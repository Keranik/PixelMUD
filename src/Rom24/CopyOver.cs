using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using static Rom24.Merc;

namespace Rom24
{
    /*
     * True copyover for Windows: hand live sockets to the child process with
     * WSADuplicateSocket instead of just restarting and dropping everyone.
     *
     * Parent (DoCopyover): starts the child, duplicates every playing client
     * socket plus the listening socket into the child process, writes the
     * handoff file, and exits. The duplicated handles keep the TCP
     * connections alive across the parent's exit, so players never disconnect.
     *
     * Child (Recover): waits for the handoff file, recreates each socket with
     * WSASocket, adopts the listening socket instead of rebinding, reloads
     * each character, and resumes.
     *
     * If duplication is unavailable (non-Windows, API failure), it degrades
     * to the old behavior: the server restarts and clients must reconnect.
     */
    public static class CopyOver
    {
        const string COPYOVER_FILE = "copyover.data";

        /* sizeof(WSAPROTOCOL_INFOW) on Windows, x86 and x64. If our managed
           layout ever drifts from this, WSADuplicateSocket would corrupt
           memory, so DoCopyover aborts instead of proceeding. */
        const int WSAPROTOCOL_INFOW_SIZE = 628;

        const uint WSA_FLAG_OVERLAPPED = 0x01;
        static readonly IntPtr INVALID_SOCKET = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential)]
        struct WsaProtocolChain
        {
            public int ChainLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public uint[] ChainEntries;
        }

        /* Must match the native WSAPROTOCOL_INFOW layout exactly. */
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WsaProtocolInfo
        {
            public uint dwServiceFlags1;
            public uint dwServiceFlags2;
            public uint dwServiceFlags3;
            public uint dwServiceFlags4;
            public uint dwProviderFlags;
            public Guid ProviderId;
            public uint dwCatalogEntryId;
            public WsaProtocolChain ProtocolChain;
            public int iVersion;
            public int iAddressFamily;
            public int iMaxSockAddr;
            public int iMinSockAddr;
            public int iSocketType;
            public int iProtocol;
            public int iProtocolMaxOffset;
            public int iNetworkByteOrder;
            public int iSecurityScheme;
            public uint dwMessageSize;
            public uint dwProviderReserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szProtocol;
        }

        [DllImport("ws2_32.dll", SetLastError = true)]
        static extern int WSADuplicateSocketW(IntPtr s, uint dwProcessId,
            ref WsaProtocolInfo lpProtocolInfo);

        [DllImport("ws2_32.dll", SetLastError = true)]
        static extern IntPtr WSASocketW(int af, int type, int protocol,
            ref WsaProtocolInfo lpProtocolInfo, uint g, uint dwFlags);

        [DllImport("ws2_32.dll", SetLastError = true)]
        static extern int WSAStartup(ushort wVersionRequested, byte[] lpWSAData);

        /*
         * The child process has never created a managed Socket, so Winsock
         * may not be initialized; raw WSASocketW calls would then fail with
         * WSANOTINITIALISED and every adopted connection would stall.
         */
        static bool EnsureWinsock()
        {
            try
            {
                /* WSADATA is <= 408 bytes; a larger buffer is safe. */
                if (WSAStartup(0x0202, new byte[512]) == 0)
                    return true;
            }
            catch { }
            try
            {
                /* Fallback: let .NET initialize Winsock via a throwaway socket. */
                using (var tmp = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp)) { }
                return true;
            }
            catch (Exception ex)
            {
                Db.log_f("Copyover: Winsock init failed: %s", ex.Message);
                return false;
            }
        }

        public static void DoCopyover(CharData ch)
        {
            if (Marshal.SizeOf<WsaProtocolInfo>() != WSAPROTOCOL_INFOW_SIZE)
            {
                Comm.send_to_char("Copyover aborted: internal protocol layout mismatch.\n\r", ch);
                Db.bug("DoCopyover: WSAPROTOCOL_INFOW size mismatch", 0);
                return;
            }

            string exe = Environment.ProcessPath
                ?? Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(exe))
            {
                Console.Error.WriteLine("do_copyover: execl: cannot locate executable");
                Comm.send_to_char("Copyover FAILED!\n\r", ch);
                return;
            }

            /* Start the child first: we need its PID to duplicate sockets into. */
            Process child;
            try
            {
                child = Process.Start(new ProcessStartInfo(exe)
                {
                    Arguments = $"{Game.port} copyover",
                    UseShellExecute = false,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("do_copyover: execl: " + ex.Message);
                Comm.send_to_char("Copyover FAILED!\n\r", ch);
                return;
            }
            if (child == null)
            {
                Console.Error.WriteLine("do_copyover: execl: cannot start child process");
                Comm.send_to_char("Copyover FAILED!\n\r", ch);
                return;
            }
            uint childPid = (uint)child.Id;

            string buf = RomString.sprintf(
                "\n\r *** COPYOVER by %s - please remain seated!\n\r", ch.name);

            var lines = new List<string>();

            /* Hand over the listening socket so the child keeps accepting on
               the same port with no rebind gap. */
            var listener = Comm.ControlSocket;
            if (listener != null && DuplicateInto(childPid, listener, out var linfo))
                lines.Add("*LISTEN " + ToBase64(linfo));

            for (var d = Game.descriptor_list; d != null;)
            {
                var och = d.original ?? d.character;
                var d_next = d.next;

                if (d.character == null || d.connected < CON_PLAYING || d.socket == null)
                {
                    Comm.write_to_descriptor(d.descriptor,
                        "\n\rSorry, we are rebooting. Come back in a few minutes.\n\r", 0);
                    Comm.close_socket(d);
                }
                else if (DuplicateInto(childPid, d.socket, out var info))
                {
                    lines.Add(ToBase64(info) + " " + och.name + " " + d.host);
                    Save.save_char_obj(och);
                    Comm.write_to_descriptor(d.descriptor, buf, 0);
                }
                else
                {
                    Comm.write_to_descriptor(d.descriptor,
                        "\n\rSorry, your connection did not survive the copyover. Please reconnect.\n\r", 0);
                    Comm.close_socket(d);
                }
                d = d_next;
            }

            lines.Add("-1");
            try
            {
                File.WriteAllLines(COPYOVER_FILE, lines);
            }
            catch (Exception ex)
            {
                Comm.send_to_char("Copyover file not writeable, aborted.\n\r", ch);
                Db.log_f("Could not write to copyover file: %s", COPYOVER_FILE);
                Console.Error.WriteLine("do_copyover:fopen: " + ex.Message);
                try { child.Kill(); } catch { }
                return;
            }

            child.Dispose();

            /*
             * Do NOT close our sockets here: the child's duplicated handles
             * keep every connection alive. Exiting closes our copies; the
             * child's copies keep the conversations going.
             */
            Environment.Exit(0);
        }

        public static void Recover()
        {
            Db.log_f("Copyover recovery initiated");

            if (!EnsureWinsock())
            {
                Db.log_f("Copyover: cannot initialize Winsock, clients cannot be recovered.");
                return;
            }

            /* Wait for the parent to finish duplicating and write the file. */
            string[] lines = null;
            for (int i = 0; i < 300 && lines == null; i++)
            {
                if (File.Exists(COPYOVER_FILE))
                {
                    try { lines = File.ReadAllLines(COPYOVER_FILE); }
                    catch { }
                }
                if (lines == null)
                    Thread.Sleep(100);
            }
            if (lines == null)
            {
                Console.Error.WriteLine("copyover_recover:fopen: handoff file never appeared");
                Db.log_f("Copyover file not found. Exitting.\n\r");
                Environment.Exit(1);
                return;
            }

            try { File.Delete(COPYOVER_FILE); } catch { }

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                var parts = raw.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;
                if (parts[0] == "-1")
                    break;

                /* Adopted listening socket: no rebind needed. */
                if (parts[0] == "*LISTEN")
                {
                    if (parts.Length > 1
                        && FromBase64(parts[1], out var linfo)
                        && AdoptSocket(linfo) is Socket ls)
                    {
                        Comm.AdoptControlSocket(ls);
                        Db.log_f("Copyover: adopted listening socket.");
                    }
                    continue;
                }

                /* Client line: <base64-protocol-info> <name> <host> */
                if (parts.Length < 2)
                    continue;
                string name = parts[1];
                string host = parts.Length > 2 ? parts[2] : "";

                if (!FromBase64(parts[0], out var info))
                    continue;
                var sock = AdoptSocket(info);
                if (sock == null)
                    continue;

                /* Probe: is the client still on the other end? */
                if (!SendRaw(sock, "\n\rRestoring from copyover...\n\r"))
                {
                    sock.Close();
                    continue;
                }
                Db.log_f("Copyover: adopted socket for %s.", name);

                var d = Recycle.new_descriptor();
                d.socket = sock;
                d.descriptor = Comm.NextDescriptorId();
                d.host = host;
                d.next = Game.descriptor_list;
                Game.descriptor_list = d;
                d.connected = CON_COPYOVER_RECOVER;

                bool fOld = Save.load_char_obj(d, name);

                if (!fOld)
                {
                    SendRaw(sock,
                        "\n\rSomehow, your character was lost in the copyover. Sorry.\n\r");
                    Comm.close_socket(d);
                }
                else
                {
                    SendRaw(sock, "\n\rCopyover recovery complete.\n\r");

                    if (d.character.in_room == null)
                        d.character.in_room = Handler.get_room_index(ROOM_VNUM_TEMPLE);

                    d.character.next = Game.char_list;
                    Game.char_list = d.character;

                    Handler.char_to_room(d.character, d.character.in_room);
                    Interp.do_function(d.character, Interp.do_look, "auto");
                    Comm.act("$n materializes!", d.character, null, null, TO_ROOM);
                    d.connected = CON_PLAYING;

                    if (d.character.pet != null)
                    {
                        Handler.char_to_room(d.character.pet, d.character.in_room);
                        Comm.act("$n materializes!.", d.character.pet, null, null, TO_ROOM);
                    }
                }
            }
        }

        static bool DuplicateInto(uint pid, Socket socket, out WsaProtocolInfo info)
        {
            info = new WsaProtocolInfo
            {
                ProtocolChain = new WsaProtocolChain { ChainEntries = new uint[7] }
            };
            try
            {
                return WSADuplicateSocketW(socket.Handle, pid, ref info) == 0;
            }
            catch
            {
                return false;
            }
        }

        static Socket AdoptSocket(WsaProtocolInfo info)
        {
            try
            {
                IntPtr h = WSASocketW(0, 0, 0, ref info, 0, WSA_FLAG_OVERLAPPED);
                if (h == INVALID_SOCKET || h == IntPtr.Zero)
                {
                    Db.log_f("Copyover: WSASocketW failed: %d",
                        Marshal.GetLastWin32Error());
                    return null;
                }
                var sh = new SafeSocketHandle(h, true);
                try
                {
                    var s = new Socket(sh);
                    s.Blocking = false;
                    return s;
                }
                catch
                {
                    sh.Dispose();
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        static bool SendRaw(Socket s, string txt)
        {
            try
            {
                var bytes = Encoding.Latin1.GetBytes(txt);
                int off = 0;
                while (off < bytes.Length)
                {
                    int n = s.Send(bytes, off, bytes.Length - off, SocketFlags.None);
                    if (n <= 0)
                        return false;
                    off += n;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        static string ToBase64(WsaProtocolInfo info)
        {
            int size = Marshal.SizeOf<WsaProtocolInfo>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(info, ptr, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return Convert.ToBase64String(bytes);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        static bool FromBase64(string b64, out WsaProtocolInfo info)
        {
            info = new WsaProtocolInfo();
            try
            {
                byte[] bytes = Convert.FromBase64String(b64);
                if (bytes.Length != Marshal.SizeOf<WsaProtocolInfo>())
                    return false;
                IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
                try
                {
                    Marshal.Copy(bytes, 0, ptr, bytes.Length);
                    info = Marshal.PtrToStructure<WsaProtocolInfo>(ptr);
                    return true;
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
