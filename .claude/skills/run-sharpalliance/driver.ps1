# Driver for SharpAlliance (Windows desktop game, SDL window). Run from the repo root with PowerShell 7:
#   pwsh -File .claude/skills/run-sharpalliance/driver.ps1 <command> [args]
#
# Commands:
#   launch                  start the built exe (waits for the main menu), logs to $env:TEMP\sa_out.txt
#   shot [file]             screenshot the game window to a PNG (default $env:TEMP\shot.png)
#   click X Y [waitMs] [file]
#                           left-click at X,Y given in pixels of the window screenshot, wait, then take a shot
#   tomap                   launch, then click through: new game -> OK -> YES -> close help -> mail OK
#                           (leaves you on the laptop, shot saved as laptop.png)
#   hire                    from the laptop desktop: hire a merc and shut down to the map screen (map.png)
#   quit                    kill the game
param(
    [Parameter(Position = 0)][string]$Cmd = "shot",
    [Parameter(Position = 1)][string]$A1,
    [Parameter(Position = 2)][string]$A2,
    [Parameter(Position = 3)][string]$A3,
    [Parameter(Position = 4)][string]$A4
)

$ErrorActionPreference = "Stop"
$tmp = $env:TEMP
$exeDir = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path "build\bin\SharpAlliance\net9.0"
$exe = Join-Path $exeDir "SharpAlliance.exe"

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class SAWin {
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, uint d, UIntPtr e);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
}
"@

function Get-GameWindow {
    $p = Get-Process SharpAlliance -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
    if (-not $p) { throw "SharpAlliance is not running (or has no window yet). Run: driver.ps1 launch" }
    $r = New-Object SAWin+RECT
    [SAWin]::GetWindowRect($p.MainWindowHandle, [ref]$r) | Out-Null
    return @{ Handle = $p.MainWindowHandle; Rect = $r }
}

function Take-Shot([string]$out) {
    if (-not $out) { $out = Join-Path $tmp "shot.png" }
    $w = Get-GameWindow
    [SAWin]::SetForegroundWindow($w.Handle) | Out-Null
    Start-Sleep -Milliseconds 300
    $r = $w.Rect
    $bmp = New-Object System.Drawing.Bitmap ($r.R - $r.L), ($r.B - $r.T)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($r.L, $r.T, 0, 0, $bmp.Size)
    $bmp.Save($out)
    $g.Dispose(); $bmp.Dispose()
    Write-Output $out
}

function Click-At([int]$x, [int]$y, [int]$waitMs, [string]$out) {
    $w = Get-GameWindow
    [SAWin]::SetForegroundWindow($w.Handle) | Out-Null
    $sx = $w.Rect.L + $x; $sy = $w.Rect.T + $y
    # move first and nudge, so the game sees a mouse move before the button goes down
    [SAWin]::SetCursorPos($sx, $sy) | Out-Null
    Start-Sleep -Milliseconds 300
    [SAWin]::SetCursorPos($sx + 1, $sy) | Out-Null
    Start-Sleep -Milliseconds 200
    [SAWin]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 150
    [SAWin]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds $waitMs
    Take-Shot $out
}

function Launch-Game {
    if (-not (Test-Path $exe)) { throw "Not built: $exe  (run: dotnet build SharpAlliance/SharpAlliance.csproj)" }
    Get-Process SharpAlliance -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Process -FilePath $exe -WorkingDirectory $exeDir `
        -RedirectStandardOutput (Join-Path $tmp "sa_out.txt") -RedirectStandardError (Join-Path $tmp "sa_err.txt")
    # the main menu shows after ~20s (loads the data libraries first)
    $deadline = (Get-Date).AddSeconds(90)
    while ((Get-Date) -lt $deadline) {
        Start-Sleep -Seconds 2
        if ((Test-Path (Join-Path $tmp "sa_out.txt")) -and (Select-String -Path (Join-Path $tmp "sa_out.txt") -Pattern "to SharpAlliance.Core.Screens.MainMenuScreen" -Quiet)) { break }
    }
    Start-Sleep -Seconds 2
    Write-Output "launched"
}

switch ($Cmd) {
    "launch" { Launch-Game }
    "shot"   { Take-Shot $A1 }
    "click"  {
        $wait = if ($A3) { [int]$A3 } else { 1500 }
        Click-At ([int]$A1) ([int]$A2) $wait $A4
    }
    "tomap"  {
        Launch-Game
        Click-At 327 318 2500 | Out-Null    # START NEW GAME
        Click-At 209 470 2500 | Out-Null    # OK on the initial settings
        Click-At 290 333 6000 | Out-Null    # YES to "novice mode?"  -> laptop opens
        Click-At 558 131 1200 | Out-Null    # close the help overlay
        Click-At 334 265 1200 (Join-Path $tmp "laptop.png") | Out-Null   # "You have new mail" OK
        Write-Output "on the laptop; click Shut Down at 72,424 to reach the map screen"
    }
    "hire"   {
        # from the laptop desktop: Web (A.I.M. opens) > Members > mercenary files > Next (a hireable merc)
        # > Contact > Hire > Transfer Funds > OK > OK, then Shut Down -> map screen
        Click-At 72 144 1500 | Out-Null     # Web
        Click-At 287 300 6000 | Out-Null    # Members
        Click-At 235 347 6000 | Out-Null    # Review the individual mercenary's file
        Click-At 475 442 3500 | Out-Null    # Next
        Click-At 375 442 3000 | Out-Null    # Contact
        Click-At 306 225 2500 | Out-Null    # HIRE
        Click-At 309 275 3000 | Out-Null    # TRANSFER FUNDS
        Click-At 347 267 2000 | Out-Null    # OK (transfer successful)
        Click-At 314 307 2000 | Out-Null    # OK (arrival message)
        Click-At 72 424 6000 (Join-Path $tmp "map.png") | Out-Null   # Shut Down
        Write-Output "on the map screen: $tmp\map.png"
    }
    "quit"   { Get-Process SharpAlliance -ErrorAction SilentlyContinue | Stop-Process -Force; Write-Output "stopped" }
    default  { throw "unknown command $Cmd" }
}
