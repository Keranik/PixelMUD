# Credits

## Licenses (required)

This C# server is a derivative of **ROM 2.4 / QuickMUD**, which descends from **Merc** and **DikuMUD**. Redistribution follows those stacked licenses. The stock texts are in `licenses/`:

| File | Lineage |
|------|---------|
| `licenses/license.doc` | DikuMUD |
| `licenses/license.txt` | Merc 2.1 |
| `licenses/rom.license` | ROM 2.4 |
| `licenses/rom.credits` | ROM credits |

Read those files for the binding terms. A running mud also needs the in-game login / `credits` / help attribution those licenses describe.

## Stock world data

`area/` and `imc/` are the stock QuickMUD content from [avinson/rom24-quickmud](https://github.com/avinson/rom24-quickmud), redistributed under the same Diku / Merc / ROM terms in `licenses/`.

## This port

The C# rewrite and Windows copyover socket handoff are part of the PixelMUD effort.

## Packages

- [CryptSharpOfficial](https://www.nuget.org/packages/CryptSharpOfficial/) — password hashing compatibility
