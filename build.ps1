param(
    [string]$Configuration = "Release",
    [string]$Profile = "Default"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

$manifest = Get-Content "$root\package\manifest.json" -Raw | ConvertFrom-Json
$name = $manifest.name
$version = $manifest.version_number

Write-Host "Building $name v$version (configuration: $Configuration, profile: $Profile)..."
dotnet build "$root\src\GtfoEZ\GtfoEZ.csproj" -c $Configuration -p:Profile=$Profile
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

$bepInEx = Join-Path $env:AppData "r2modmanPlus-local\GTFO\profiles\$Profile\BepInEx"
$builtDll = Join-Path $bepInEx "plugins\$name\$name.dll"
if (-not (Test-Path $builtDll)) { throw "Could not find built DLL at $builtDll. Check -Profile matches your mod manager profile." }

Copy-Item $builtDll "$root\package\$name.dll" -Force

$distDir = Join-Path $root "dist"
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
$zipPath = Join-Path $distDir "$name-$version.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Compress-Archive -Path "$root\package\*" -DestinationPath $zipPath
Write-Host "Packaged: $zipPath"
