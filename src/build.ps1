$ErrorActionPreference='Stop'
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if(-not (Test-Path -LiteralPath $compiler)){$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'}
& $compiler /nologo /target:winexe /platform:anycpu /codepage:65001 /optimize+ ("/out:"+(Join-Path $PSScriptRoot 'Msu2Player.exe')) ("/win32icon:"+(Join-Path $PSScriptRoot 'icons\icon-C.ico')) ("/resource:"+(Join-Path $PSScriptRoot 'icons\icon-C.ico')+',player-icon.ico') ("/resource:"+(Join-Path $PSScriptRoot 'icons\icon-C.png')+',player-logo.png') /reference:System.Windows.Forms.dll /reference:System.Drawing.dll (Join-Path $PSScriptRoot 'Msu2Player.cs')
if($LASTEXITCODE -ne 0){throw 'Build failed'}
