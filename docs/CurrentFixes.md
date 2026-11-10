# Current fixes in Umbra Revolutions

Updated 10 November 2026. This lists the fixes in release 3.2.0.

Release 3.2.0: BepInEx only, with plain file names. The game is the Windows version of Demeo 1.42 (played on a Quest 2 connected to the PC) with BepInEx 5.4.22. Only the host runs the mod; guests play unmodded.

## Fixes in Revolutions

| # | Fix | What it does | Status |
|---|-----|--------------|--------|
| 1 | Demeo 1.42 support | The mod was rebuilt for the current game version. | In use since 19 Oct |
| 2 | Energy Potion works right away | The potion's class ability is given the moment the potion takes effect, not on the next turn. | Confirmed working |
| 3 | Area damage is reset | All four area-damage values (direct, critical, splash, critical splash) go back to normal when the ruleset ends. | In use |
| 4 | No host freeze from summons | Summon lists skip pieces that have no model in the game. Before, a summon of the Poisonous Rat created a piece with nothing to show and stalled the host's turn. | In use; no freezes since |
| 5 | Poisonous Rat stand-in | The ruleset's Nature's Call summons a Poisonous Rat, which Demeo 1.42 no longer has. A normal Rat is summoned instead, with the Poisonous Rat's stats and abilities from the ruleset. Only Rats on the heroes' team get these; enemy Rats are unchanged. | Confirmed working. In the 9 Nov log, the summoned Rat had the ruleset's 2 health. Its bite has not shown in a log yet. |
| 6 | Settings don't leak into later games | The level-order rules and the precise health and attack settings switch off when a game ends. Before, they stayed on in later games in the same session, even without Revolutions. | In use |
| 7 | Hunter one-time cards | When two one-time cards sit next to each other, both are removed, not just the first. | In use |
| 8 | No setup error on two cards | Vigor Potion and Free AP have no stat bonus, so the "maxed-out card" setting skips them with a warning instead of throwing an error. | In use |
| 9 | Card art for Revolutions cards | About 15 cards that Revolutions creates (Grab, Net, Heavy Leap, Energy Potion and others) have no picture in Demeo. They now use a similar card's picture. This stops the error the game logged on every hand redraw (about 540 in five minutes), which probably caused some host stutter. | Loaded in a game on 9 Nov (the log reports art added for 15 cards) and no missing-art errors were logged; not yet checked on screen |

Fix 4's skip only matters if a summon list names another missing piece. With the current ruleset it never runs (only the Poisonous Rat is missing, and fix 5 covers it). It is kept as a safety net.

## Fixes in the helper mods

| Mod | Fix | Status |
|-----|-----|--------|
| RoomCode 1.3.1 | No longer skips the first room code. It proposes the same code until the server says it is taken. | Working (9 Nov) |
| FreeCamera | Mouse camera controls work again after the game update. Only matters when playing on PC without a headset. | In use |

## Guests

None of these fixes changes the full-game syncs Revolutions sends; every sync goes out as in the original mod. Fixes 4 and 5 run on the host, and the results reach guests through the normal syncs. Fix 9 only changes the host's card pictures.
