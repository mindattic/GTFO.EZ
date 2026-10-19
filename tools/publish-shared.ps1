#Requires -Version 5.1
<#
.SYNOPSIS
    Copies GTFO.EZ's shared code (src/GTFO.EZ/Shared) into the other GTFO.* mods.

.DESCRIPTION
    GTFO.EZ owns the code every GTFO.* mod carries a copy of: the Insert/Delete infinite ammo and
    health toggles (SharedOwner + InfiniteToggles, in every mod), on-screen messages (MessageHelper, in
    every mod) and kits as consumables (KitLoot,
    GTFO.EZ.AbsurdItems only - GTFO.EZ keeps the source but doesn't compile it). This
    writes each file into <repo>/src/<repo>/Shared/ with a header saying where it came from, in the order
    GTFO.EZ.AbsurdWeapons, GTFO.EZ.AbsurdMoves, GTFO.EZ.AbsurdItems, GTFO.CrashTestDummies. The copies are never
    edited by hand - change GTFO.EZ's and publish again. A generated copy that's no longer published
    to a repo (a file renamed or retired here) is deleted from it.

.PARAMETER Check
    Change nothing; list every copy that's missing, differs from GTFO.EZ's or is no longer published,
    and exit 1 if any are.

.PARAMETER Only
    Only these target repos (e.g. -Only GTFO.EZ.AbsurdItems,GTFO.EZ.AbsurdMoves) - for when another repo is
    mid-change and mustn't be written to yet. Every target by default.
#>
[CmdletBinding()]
param([switch]$Check, [string[]]$Only)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$workspace = Split-Path -Parent $repoRoot
$source = Join-Path $repoRoot 'src\GTFO.EZ\Shared'

$everyMod = @('SharedOwner.cs', 'InfiniteToggles.cs', 'MessageHelper.cs')
$targets = [ordered]@{
    'GTFO.EZ.AbsurdWeapons' = $everyMod
    'GTFO.EZ.AbsurdMoves'   = $everyMod
    'GTFO.EZ.AbsurdItems'   = $everyMod + @('KitLoot.cs')
    'GTFO.CrashTestDummies' = $everyMod
}

if ($Only) {
    $unknown = @($Only | Where-Object { -not $targets.Contains($_) })
    if ($unknown.Count -gt 0) { throw "Not a publish target: $($unknown -join ', ')" }
}

$commit = (git -C $repoRoot rev-parse --short HEAD).Trim()
$stale = @()

foreach ($repo in $targets.Keys) {
    if ($Only -and $Only -notcontains $repo) { continue }
    $folder = Join-Path $workspace $repo
    $dest = Join-Path $folder "src\$repo\Shared"
    if (-not (Test-Path $folder)) { Write-Warning "$repo isn't in the workspace - skipped."; continue }

    foreach ($file in $targets[$repo]) {
        $body = [IO.File]::ReadAllText((Join-Path $source $file))
        $target = Join-Path $dest $file
        # The header's commit changes every publish, so a copy is compared on its body alone.
        $existing = if (Test-Path $target) { [IO.File]::ReadAllText($target) } else { $null }
        $existingBody = if ($existing) { ($existing -split "`n", 3)[2] } else { $null }

        if ($existingBody -eq $body) { continue }
        if ($Check) { $stale += "$repo/$file"; continue }

        $header = "// Generated from GTFO.EZ src/GTFO.EZ/Shared/$file ($commit) - don't edit here.`n" +
                  "// Change GTFO.EZ's copy and run GTFO.EZ/tools/publish-shared.ps1.`n"
        New-Item -ItemType Directory -Force $dest | Out-Null
        [IO.File]::WriteAllText($target, $header + $body, (New-Object Text.UTF8Encoding $false))
        Write-Host "Published $file -> $repo"
    }

    # Generated copies this repo no longer gets (renamed or retired here). Only files carrying the
    # generated header are touched, never a repo's own code.
    if (Test-Path $dest) {
        foreach ($old in Get-ChildItem $dest -Filter *.cs) {
            if ($targets[$repo] -contains $old.Name) { continue }
            $reader = New-Object IO.StreamReader $old.FullName
            try { $first = $reader.ReadLine() } finally { $reader.Dispose() }
            if ($first -notlike '// Generated from GTFO.EZ src/GTFO.EZ/Shared/*') { continue }
            if ($Check) { $stale += "$repo/$($old.Name) (no longer published)"; continue }
            Remove-Item $old.FullName
            Write-Host "Removed $($old.Name) from $repo (no longer published)"
        }
    }
}

if ($Check) {
    if ($stale.Count -gt 0) { $stale | ForEach-Object { Write-Host "Out of date: $_" }; exit 1 }
    Write-Host 'Every shared copy matches GTFO.EZ.'
}
