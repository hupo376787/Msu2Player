param([switch]$Download)
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$output = Join-Path $PSScriptRoot 'output'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
& $compiler /nologo /target:exe /main:FfmpegTests /codepage:65001 `
    ("/out:" + (Join-Path $output 'FfmpegTests.exe')) `
    /reference:System.Windows.Forms.dll /reference:System.Drawing.dll `
    /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll `
    (Join-Path $project 'src\Msu2Player.cs') (Join-Path $PSScriptRoot 'FfmpegTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'FFmpeg test build failed' }
$arguments = @()
if ($Download) { $arguments += '--download' }
& (Join-Path $output 'FfmpegTests.exe') @arguments
if ($LASTEXITCODE -ne 0) { throw 'FFmpeg tests failed' }
