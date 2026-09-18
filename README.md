# PixelMUD

A **ROM 2.4 / QuickMUD** MUD server rewritten in **C#** for .NET 8.

This is a faithful port of the classic QuickMUD engine so it can run as a modern .NET process. Classic telnet clients still connect the usual way.

## Build

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet build src/Rom24/Rom24.csproj -c Release
```

Then start the server by running the built executable (not `dotnet run`). Copyover starts a new child process of that same binary; launching via `dotnet run` breaks that handoff.

```bash
# Linux / macOS (path may vary with RID)
./src/Rom24/bin/Release/net8.0/Rom24 <port>

# Windows
.\src\Rom24\bin\Release\net8.0\Rom24.exe <port>
```

Run with a QuickMUD-compatible data tree in the working directory (area list, areas, player dir, helps) from a stock QuickMUD/ROM distribution you are licensed to use.

## Licenses

This is a DikuMUD → Merc → ROM / QuickMUD derivative. Stock license texts are in [`licenses/`](licenses/). See [CREDITS.md](CREDITS.md).

## Notes

- The server code tracks QuickMUD C closely, including quirks.
- On Windows, copyover can hand live sockets to a new process so players stay connected (`CopyOver.cs`). Elsewhere it may fall back to restart-and-reconnect.
