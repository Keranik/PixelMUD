# PixelMUD

A **ROM 2.4 / QuickMUD** MUD server rewritten in **C#** for .NET 8.

This is a faithful port of the classic QuickMUD engine so it can run as a modern .NET process. Classic telnet clients still connect the usual way. Stock QuickMUD areas and helps are included under `area/` so a fresh clone can boot.

## Build

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet build src/Rom24/Rom24.csproj -c Release
```

Then start the server by running the built executable (not `dotnet run`). Copyover starts a new child process of that same binary; launching via `dotnet run` breaks that handoff.

```bash
# From the repo root (so area/ and player/ resolve):
./src/Rom24/bin/Release/net8.0/Rom24 <port>

# Windows
.\src\Rom24\bin\Release\net8.0\Rom24.exe <port>
```

The process walks up from the binary to find the repo root (a directory that contains `area/`), then loads `area/` and writes players under `player/`.

## Layout

- `src/Rom24/` — server
- `area/` — stock QuickMUD world, helps, socials, music (from [avinson/rom24-quickmud](https://github.com/avinson/rom24-quickmud))
- `player/` — player files created at runtime (empty in git)
- `imc/` — stock IMC config files
- `licenses/` — Diku / Merc / ROM license texts

## Licenses

This is a DikuMUD → Merc → ROM / QuickMUD derivative. Stock license texts are in [`licenses/`](licenses/). See [CREDITS.md](CREDITS.md). Areas and helps ship under those same lineage terms.

## Notes

- The server code tracks QuickMUD C closely, including quirks.
- On Windows, copyover can hand live sockets to a new process so players stay connected (`CopyOver.cs`). Elsewhere it may fall back to restart-and-reconnect.
