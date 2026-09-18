using static Rom24.Merc;

namespace Rom24
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var root = FindRepoRoot();
            int port = 4000;
            if (args.Length > 0)
            {
                if (!Interp.is_number(args[0]))
                {
                    Console.Error.WriteLine("Usage: {0} [port #]",
                        AppDomain.CurrentDomain.FriendlyName);
                    return 1;
                }
                port = Interp.atoi(args[0]);
                if (port <= 1024)
                {
                    Console.Error.WriteLine("Port number must be above 1024.");
                    return 1;
                }
                if (args.Length > 1 && !RomString.str_cmp(args[1], "copyover"))
                    Game.fCopyOver = true;
            }
            Boot(root, port);
            if (Game.fCopyOver)
                ActWiz.copyover_recover();
            Comm.game_loop();
            return 0;
        }

        public static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "rom24-master", "area")))
                    return dir.FullName;
                if (Directory.Exists(Path.Combine(dir.FullName, "area")) && dir.Name == "rom24-master")
                    return dir.Parent.FullName;
                dir = dir.Parent;
            }
            return Directory.GetCurrentDirectory();
        }

        public static void Boot(string root, int port = 4000)
        {
            Game.area_dir = Path.Combine(root, "rom24-master", "area");
            Game.player_dir = Path.Combine(root, "rom24-master", "player");
            Game.port = port;
            Game.current_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Game.str_boot_time = RomString.ctime(Game.current_time);
            Directory.SetCurrentDirectory(Game.area_dir);
            Db.boot_db();
            Db.log_f("ROM is ready to rock on port %d (%s).", Game.port, Game.mud_ipaddress);
        }


    }
}
