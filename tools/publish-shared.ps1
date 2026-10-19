#Requires -Version 5.1
<#
.SYNOPSIS
    Copies GTFO.EZ's shared code (src/GTFO.EZ/Shared) into the other GTFO.* mods.

.DESCRIPTION
    GTFO.EZ owns the code every GTFO.* mod carries a copy of: the Insert/Delete infinite ammo and
    health toggles (SharedOwner + InfiniteToggles + WardenMessage, in every mod) and kits as consumables (KitLoot,
    GTFO.AbsurdWeapons only). This writes each file into <repo>/src/<repo>/Shared/ with a header
    saying where it came from, in the order GTFO.AbsurdWeapons, GTFO.AbsurdMoves,
    GTFO.CrashTestDummies. The copies are never edited by hand - change GTFO.EZ's and publish again.

.PARAMETER Check
    Change nothing; list every copy that's missing or differs from GTFO.EZ's, and exit 1 if any do.
#>
[CmdletBinding()]
param([switch]$Check)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$workspace = Split-Path -Parent $repoRoot
$source = Join-Path $repoRoot 'src\GTFO.EZ\Shared'

$everyMod = @('SharedOwner.cs', 'InfiniteToggles.cs', 'WardenMessage.cs')
$targets = [ordered]@{
    'GTFO.AbsurdWeapons'    = $everyMod + @('KitLoot.cs')
    'GTFO.AbsurdMoves'      = $everyMod
    'GTFO.CrashTestDummies' = $everyMod
}

$commit = (git -C $repoRoot rev-parse --short HEAD).Trim()
$stale = @()

foreach ($repo in $targets.Keys) {
    $dest = Join-Path $workspace "$repo\src\$repo\Shared"
    if (-not (Test-Path (Join-Path $workspace $repo))) { Write-Warning "$repo isn't in the workspace - skipped."; continue }

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
}

if ($Check) {
    if ($stale.Count -gt 0) { $stale | ForEach-Object { Write-Host "Out of date: $_" }; exit 1 }
    Write-Host 'Every shared copy matches GTFO.EZ.'
}
