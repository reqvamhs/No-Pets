# No Pets

A [BepInEx 5](https://github.com/BepInEx/BepInEx) plugin for Hearthstone that hides **opponent battlefield pets** (and their corner platforms) during games.

Pets sit on the board next to the hero, animate, react, and occupy a corner platform. If you'd rather have a clean board, this mod keeps the opponent's pet from ever appearing — no pet model, no platform — in every game mode, Battlegrounds included. Your own pet stays visible by default; an optional toggle hides it too (pet XP still accrues normally — only the visual is skipped). Collection previews are never affected, so browsing and inspecting pets works normally.

Purely cosmetic and strictly local — only what *your* client renders changes. The opponent sees their pet as usual.

> ⚠️ **Disclaimer:** All client-side mods technically violate Blizzard's Terms of Service and carry a ban risk. Use at your own risk. This project is not affiliated with or endorsed by Blizzard Entertainment.

## How it works

The game's pet board controller checks its own creation blocker (`IsCreationBlocked`) as the very first step of pet creation — it's the same mechanism Hearthstone itself uses to suppress pets in certain UI states. This plugin installs a [Harmony](https://github.com/BepInEx/HarmonyX) prefix that answers "blocked" for the configured sides, making every creation attempt a clean no-op on a fully supported code path (the game's pet event handling explicitly treats "no pet object" as a first-class state). The corner platform is a separate corner-decoration system driven by per-side contexts; a second prefix zeroes the pet entry for hidden sides before it is applied. Both patches act only in actual gameplay.

Note for Battlegrounds duos: your teammate's pet counts as "not your own", so it is hidden under the default settings.

## Installation

You need BepInEx 5 in your Hearthstone folder — via **either** route — then the mod DLL.

**Option A — via Firestone (easiest if you already use it):**
1. With Hearthstone closed, open Firestone → Settings → General → Mods and enable mods.
   This installs Firestone's integrated BepInEx into the game folder.
2. Launch Hearthstone once and quit, so `<GameDir>\BepInEx\plugins\` gets created.

**Option B — plain BepInEx:**
1. Download [BepInEx 5.4.x **x64**](https://github.com/BepInEx/BepInEx/releases) and extract
   the zip directly into your Hearthstone folder (next to `Hearthstone.exe`), so you end up
   with `<GameDir>\winhttp.dll` and `<GameDir>\BepInEx\`.
2. Launch Hearthstone once and quit.

**Then, for both routes:**
1. Download `HsNoPets.dll` from [Releases](../../releases) and drop it into
   `<GameDir>\BepInEx\plugins\`.
2. Launch Hearthstone. Verify by finding `No Pets ... loaded.` in
   `<GameDir>\BepInEx\LogOutput.log`, or by queueing into an opponent with a pet equipped.

## Configuration (optional)

Edit `<GameDir>\BepInEx\config\HsNoPets.cfg` (created on first launch):

```ini
[Features]
## Master toggle for the plugin.
HidePets = true

## Hide the opponent pet and its corner platform during games.
HideOpponentPet = true

## Also hide your own pet and its corner platform during games.
HideOwnPet = false
```

Looking to skip the pet **end-of-game** sequence too? That's a separate companion mod:
[Skip-Pet-End-Screen](https://github.com/reqvamhs/Skip-Pet-End-Screen). The two are independent — install either or both.

## Building from source

Requirements: a .NET SDK (8/9/10), BepInEx 5 installed in the game folder (the project references its DLLs from there).

1. Clone the repo.
2. Edit `<GameDir>` in `HsNoPets.csproj` to your Hearthstone install path.
3. `dotnet build -c Release` — the post-build step copies the DLL into `BepInEx\plugins` automatically.

No game files are included in this repository; the project compiles against `Assembly-CSharp.dll` from your own installation.

## Compatibility

- Built and verified against the July 2026 Hearthstone build.
- Hearthstone patches can rename or change the hooked methods (`PetControllerBoard.IsCreationBlocked`, `CornerSpellReplacementManager.UpdateCornerSpellReplacements`). If the mod stops working after a game update, check `LogOutput.log` for Harmony errors and watch this repo for an updated release.
- Coexists with other BepInEx/Firestone mods, including the author's other Hearthstone mods.

## Uninstall

Delete `HsNoPets.dll` from `BepInEx\plugins`. To remove BepInEx entirely, delete `winhttp.dll` from the game folder.

## License

[MIT](LICENSE)
