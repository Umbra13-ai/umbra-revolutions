# Changelog

## Umbra Revolutions 3.2.0 (10 November 2026)

First release of this fork. The previous Demeo Revolutions release was 3.1.1.

**Changes since 3.1.1**

**What players will notice**

- **Demeo 1.42 support.** The plugins were rebuilt and adjusted for the current version of the game.
- **RoomCode 1.3.1:** fixes a bug in the previous version that ignored the first code in the config file.
- **FreeCamera: mouse camera controls work again** after the 1.42 update. This only matters when playing on a PC without a headset.
- **Energy Potion:** the class ability card now arrives the moment the potion is consumed, not at the next turn.

**Fixes for problems that could stall or confuse a game**

- **Hunter: Nature's Call no longer freezes the host's turn.** The ruleset summons a Poisonous Rat, which Demeo 1.42 no longer has, so the summon failed and the host's turn stopped advancing. It now summons a normal Rat with the Poisonous Rat's stats and abilities from the ruleset. (The poison bite has not yet shown up in a game log.)
- **Settings no longer carry over into the next game.** Two things in Revolutions are switched on for a game and were never switched off when it ended:
  - *Level order.* Revolutions builds its floors from different adventures in its own order. That switch stayed on, so the next game in the same session could use Revolutions' level order even under a different ruleset or none.
  - *Exact monster health and attack.* To give each enemy its own health and damage, Revolutions turns off the game's automatic scaling of monsters. That was never turned back on, so monsters in later games stayed unscaled until the game was restarted.

  Both are now restored when the game ends.
- **Area damage is put back when the ruleset ends.** Some rules change how hard an ability hits. Every ability has four damage numbers: a direct hit, a critical hit, *splash* (the damage to creatures standing next to the target, such as in an explosion) and critical splash. The plugin saved only the first two before changing them, so at the end of the game it could restore only those, hit an error, and leave the splash numbers at the ruleset's values. Explosions and other area attacks in a later game could then hurt neighboring creatures more or less than the game intends, until the game was restarted. All four are now saved and restored.
- **Hunter one-time cards:** a critical hit on the last action of a level gives the Hunter a Freeze Arrow and a Bone as one-time cards. After using both, only one of them was removed and the other stayed in the Hunter's hand. Both are now removed. Bones from drops and from other critical hits are not affected.
- **No setup error on cards without a stat bonus.** The Vigor Potion and Free AP cards have no stat bonus, so the "regain the card if the stat is already maxed out" setting triggered errors during game setup that may cause the game to freeze. Those cards are now skipped.

**Card art**

- **Card pictures for the cards Revolutions creates.** About 15 cards that Revolutions adds (Grab, Net, Heavy Leap and others) have no picture in Demeo. Each time the host's hand was redrawn, the game failed to find those pictures and logged an error, about 540 in five minutes in one game. That could cause stutter for the host over Air Link. The plugin now uses a similar card's picture when the game has none. This only affects the host.

Upgrading and install steps are on the [release page](https://github.com/Umbra13-ai/umbra-revolutions/releases/latest) and in the README.

**November 2026 (Umbra's fork): BepInEx only, plain file names.** MelonLoader support has been removed from every plugin in this repository; it had been phased out and no longer works with the current game. The file-name suffix that told the BepInEx and MelonLoader versions apart (`.BIE.dll` / `.ML.dll`) has gone with it. Before installing these builds, delete the old `.BIE.dll` (and any `.ML.dll`) files from `BepInEx/plugins`, or BepInEx loads each plugin twice. See the README's "Upgrading from an earlier build" and each plugin's changelog.

Changelogs can be found in each mod's directory.

### [HouseRules.Core Changelog](HouseRules.Core/CHANGELOG.md)

### [HouseRules.Essentials Changelog](HouseRules.Essentials/CHANGELOG.md)

### [HouseRules.Configuration Changelog](HouseRules.Configuration/CHANGELOG.md)

### [SkipIntro Changelog](SkipIntro/CHANGELOG.md)

### [RoomFinder Changelog](RoomFinder/CHANGELOG.md)

### [RoomCode Changelog](RoomCode/CHANGELOG.md)
