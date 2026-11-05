---
name: run-sharpalliance
description: Build, run, start, launch, drive and screenshot the SharpAlliance game (C# port of Jagged Alliance 2) on Windows. Use to see a change working in the real game window - click through menus, take screenshots, check a screen renders.
---

# Run SharpAlliance

SharpAlliance is a Windows SDL desktop game (a C# port of Jagged Alliance 2). It is driven by
`driver.ps1` in this directory: it launches the exe, takes screenshots of the game window and sends
real mouse clicks. Paths below are relative to the repo root (`E:\Projects\SharpAlliance`).
Run everything with `pwsh -NoProfile -File .claude/skills/run-sharpalliance/driver.ps1 <command>`.

## Prerequisites

- Windows with .NET SDK 9 or later (the exe targets net9.0).
- The original game's data files. The game reads them from the `DataDirectory` in
  `SharpAlliance/SharpAlliance.json` (`C:\SharpAlliance\Data`, which holds `*.slf` libraries).
  Without them it cannot start.
- A real, visible desktop session. The driver uses `CopyFromScreen` and `mouse_event`, so the game window
  must be on screen and not covered by another window while a shot or click runs.

## Build

```powershell
dotnet build SharpAlliance/SharpAlliance.csproj -v q
```

Output: `build/bin/SharpAlliance/net9.0/SharpAlliance.exe`. (A `dotnet test` of the whole solution fails
to restore the Web projects, which target net8.0. Build `SharpAlliance/SharpAlliance.csproj` or
`SharpAlliance.SDL` instead.)

## Run (agent path)

```powershell
pwsh -NoProfile -File .claude/skills/run-sharpalliance/driver.ps1 tomap       # launch and click to the laptop
pwsh -NoProfile -File .claude/skills/run-sharpalliance/driver.ps1 shot out.png
pwsh -NoProfile -File .claude/skills/run-sharpalliance/driver.ps1 click 72 424 4000 map.png   # Shut Down -> map screen
pwsh -NoProfile -File .claude/skills/run-sharpalliance/driver.ps1 quit
```

Hiring a merc (from the laptop desktop, takes about 40 seconds; ends on the map screen with the merc in transit):

```powershell
pwsh -NoProfile -File .claude/skills/run-sharpalliance/driver.ps1 hire
```

Then start the clock with the time-compression "more" button (`click 545 493`, each click is one step faster:
5 min, 30 min, 60 min per real second). The merc arrives at about 07:30 on day 1, time stops by itself,
and the merc's assignment changes from "In Trans" to "On Duty" in sector A9.

Then **read the PNG** (the Read tool shows images). Coordinates for `click X Y` are pixels in the
window screenshot (the game picture is 640x480 and starts at about x=8, y=31 inside the shot).

Known click positions (window-screenshot pixels):

| Screen | Click | X, Y |
|---|---|---|
| Main menu | START NEW GAME | 327, 318 |
| Initial game settings | OK | 209, 470 |
| "Novice mode?" box | YES | 290, 333 |
| Laptop help overlay | close X | 558, 131 |
| "You have new mail" box | OK check | 334, 265 |
| Laptop | Shut Down (goes to the map screen) | 72, 424 |

`tomap` runs the first five of these and leaves you on the laptop; `click 72 424` then opens the map.
Game console output (screens changing, graphics loaded, exceptions) is in `$env:TEMP\sa_out.txt`
and `sa_err.txt`. Read them when a screen looks wrong.

Verified this way: main menu, settings screen, laptop, and the map screen (map picture, town names,
border buttons, bottom panel, clock text).

## Run (human path)

`build/bin/SharpAlliance/net9.0/SharpAlliance.exe`, or `dotnet run --project SharpAlliance`. A window
opens; close it to quit. Useless headless.

## Gotchas

- The main menu takes about 20 seconds to appear (it loads the data libraries first). `launch` waits for
  the log line `to SharpAlliance.Core.Screens.MainMenuScreen` and then 2 more seconds.
- The game must receive a mouse move before a click, or the click is ignored. `click` moves the cursor,
  nudges it by 1 pixel, then presses, which is what the game needs.
- Rebuilding while the game is running fails (the exe is locked): run `quit` first.
- The map picture used to be black because the `.pcx` loader made every pixel transparent. That is fixed in
  `PCXImageFileLoader.CreateIndexedImages`. If a `.pcx` picture is blank again, look there.
- Backslashes in file paths inside C# strings must be doubled (`"INTERFACE\\boxes.sti"`). A single `\b` or
  `\t` compiles and silently loads the wrong file.
- Each `hire` run starts a new game, so the balance, the A.I.M. roster and `TEMP/finances.dat` in the exe folder start
  over only if you delete the file. A stale `TEMP/finances.dat` from an older build can give a wrong balance.
- A.I.M. page loads show a "Downloading" bar for a few seconds; the driver's waits cover it, but on a slow
  machine raise the wait on the `click` that follows (a click during the bar is ignored).
- Which merc `Next` lands on varies; some show "On Assignment" and can't be hired. If a hire step fails, look at
  the last shot and press `Next` again.

## Troubleshooting

- `SharpAlliance is not running (or has no window yet)`: run `launch`, wait for it to return.
- `Not built: ...SharpAlliance.exe`: run the Build step above.
- Screenshot shows another application: something covered the game window. Close it and run `shot` again;
  the driver brings the game to the front before each shot.
