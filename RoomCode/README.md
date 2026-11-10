# RoomCode

Set your own room code.

After installing the mod, run the game once. Its settings file is then in the Demeo game directory at
`BepInEx/config/com.orendain.demeomods.roomcode.cfg`:

```ini
[General]
Enabled = true
Codes = 8888, 7777, 1234
```

`Enabled`: `true` to use the mod, `false` to switch it off.

`Codes`: the room codes you would like to use, in order of preference, separated by commas.

If none of the room codes are available, the mod will fall back to Demeo's
random room code generation.

The first code is used whenever it is free. The mod moves to the next code
only when the server reports the current one as taken, and starts again at
the top of the list each time you create a room. Versions before 1.3.1
skipped the first code, which is why some configurations list it twice.


**Example configuration:**
```toml
[RoomCode]
enabled = true
codes = ["8888", "7777", "1234"]
```
