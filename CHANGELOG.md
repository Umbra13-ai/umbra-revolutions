# Changelog

## Umbra Revolutions 3.2.0 (10 November 2026)

First release of this fork. The previous Demeo Revolutions release was 3.1.1.

**Changes since 3.1.1**

- **Demeo 1.42 support.** The plugins were rebuilt for the current game version.
- **BepInEx only, plain file names.** MelonLoader support is gone and the `.BIE.dll` suffix with it. See "Upgrading from an earlier build" in the README: delete the old `.BIE.dll` files before installing.
- **Energy Potion works right away.** The potion's class ability is given the moment it takes effect, not on the next turn.
- **Area damage is reset** when the ruleset ends (all four values).
- **No host freeze from summons.** Summon lists skip pieces that have no model in the game.
- **Poisonous Rat stand-in.** Nature's Call summons a normal Rat with the Poisonous Rat's stats and abilities, because Demeo 1.42 no longer has the Poisonous Rat.
- **Settings no longer leak into later games** (level-order rules, precise health and attack).
- **Hunter one-time cards:** two adjacent one-time cards are both removed.
- **No setup error** on cards without a stat bonus (Vigor Potion, Free AP).
- **Card art for the cards Revolutions creates** (about 15) that have none in Demeo, which stops an error logged on every hand redraw.
- **RoomCode 1.3.1** no longer skips the first room code.
- **FreeCamera:** mouse camera controls work again after the game update (PC without a headset).

Details are in `docs/CurrentFixes.md`. LogArchiver, a separate plugin that keeps every session's logs, now lives in its own repository: https://github.com/Umbra13-ai/umbra-logarchiver

**November 2026 (Umbra's fork): BepInEx only, plain file names.** MelonLoader support has been removed from every plugin in this repository; it had been phased out and no longer works with the current game. The file-name suffix that told the BepInEx and MelonLoader versions apart (`.BIE.dll` / `.ML.dll`) has gone with it. Before installing these builds, delete the old `.BIE.dll` (and any `.ML.dll`) files from `BepInEx/plugins`, or BepInEx loads each plugin twice. See the README's "Upgrading from an earlier build" and each plugin's changelog.

Changelogs can be found in each mod's directory.

### [HouseRules.Core Changelog](HouseRules.Core/CHANGELOG.md)

### [HouseRules.Essentials Changelog](HouseRules.Essentials/CHANGELOG.md)

### [HouseRules.Configuration Changelog](HouseRules.Configuration/CHANGELOG.md)

### [SkipIntro Changelog](SkipIntro/CHANGELOG.md)

### [RoomFinder Changelog](RoomFinder/CHANGELOG.md)

### [RoomCode Changelog](RoomCode/CHANGELOG.md)
