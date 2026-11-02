<#
MSU2 host driver, Windows PowerShell 5.1 / PowerShell 7.
Examples:
  .\Msu2Screen.ps1 -Mode Info
  .\Msu2Screen.ps1 -Mode Pattern
  .\Msu2Screen.ps1 -Mode Image -ImagePath C:\Pictures\photo.png
Only display RAM is written. No Flash, firmware or SFR writes.
#>
[CmdletBinding()]
param(
 [ValidateSet('Info','Pattern','Image')][string]$Mode='Info',
 [string]$Port='COM13',
 [string]$ImagePath,
 [ValidateRange(1,240)][int]$Width=160,
 [ValidateRange(1,240)][int]$Height=80,
 [ValidateRange(0,3600)][int]$HoldSeconds=30
)
$ErrorActionPreference='Stop'
$trace=[Collections.Generic.List[string]]::new()
$logPath=Join-Path $PSScriptRoot ('MSU2-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff')+'.log')
function Log([string]$s){$s=(Get-Date -Format 'HH:mm:ss.fff')+' '+$s;$trace.Add($s);Write-Host $s}
# Verify the selected port's USB identity before sending anything.
$matched=$false
$root='HKLM:\SYSTEM\CurrentControlSet\Enum\USB\VID_1A86&PID_FE0C&MI_00'
if(Test-Path -LiteralPath $root){
 foreach($key in Get-ChildItem -LiteralPath $root){
  $props=Get-ItemProperty -LiteralPath ($key.PSPath+'\Device Parameters') -ErrorAction SilentlyContinue
  if($props.PortName -eq $Port){$matched=$true}
 }
}
if(-not $matched){throw "Port $Port does not match the verified USB identity 1A86:FE0C MI_00."}
if($Mode -eq 'Pattern'){$ImagePath=Join-Path $PSScriptRoot 'COM13-四色测试.png';$Mode='Image'}
if($Mode -eq 'Image' -and (-not $ImagePath -or -not (Test-Path -LiteralPath $ImagePath -PathType Leaf))){throw 'Specify an existing -ImagePath.'}
$serial=[IO.Ports.SerialPort]::new($Port,19200,[IO.Ports.Parity]::None,8,[IO.Ports.StopBits]::One)
$serial.DtrEnable=$true;$serial.RtsEnable=$false;$serial.ReadTimeout=100;$serial.WriteTimeout=5000
function Receive([int]$timeout){
 $stream=[IO.MemoryStream]::new();$timer=[Diagnostics.Stopwatch]::StartNew();$last=0
 try {
  while($timer.ElapsedMilliseconds -lt $timeout){
   if($serial.BytesToRead -gt 0){
    $buf=[byte[]]::new($serial.BytesToRead);$n=$serial.Read($buf,0,$buf.Length)
    $stream.Write($buf,0,$n);$last=$timer.ElapsedMilliseconds
   } elseif($stream.Length -gt 0 -and $timer.ElapsedMilliseconds-$last -ge 80){break}
   Start-Sleep -Milliseconds 5
  }
  return ,$stream.ToArray()
 } finally {$stream.Dispose()}
}
function Send([byte[]]$bytes){
 Log ('TX '+[BitConverter]::ToString($bytes));$serial.Write($bytes,0,$bytes.Length)
}
function Ack([byte[]]$command){
 $serial.DiscardInBuffer();Send $command;$reply=Receive 2000
 Log ('RX '+[BitConverter]::ToString($reply))
 # ACK is the same 6-byte command; permit additional bytes before/after it.
 $expected=[BitConverter]::ToString($command)
 if(-not [BitConverter]::ToString($reply).Contains($expected)){throw "Missing ACK for $expected"}
}
function Window([int]$x,[int]$y,[int]$w,[int]$h){
 Send ([byte[]](2,0,($x -shr 8),($x -band 255),($y -shr 8),($y -band 255)))
 Send ([byte[]](2,1,($w -shr 8),($w -band 255),($h -shr 8),($h -band 255)))
}
function Fill([int]$x,[int]$y,[int]$w,[int]$h,[int]$color){
 Window $x $y $w $h
 Ack ([byte[]](2,3,11,($color -shr 8),($color -band 255),0))
}
try {
 $serial.Open();Log "OPEN $Port 19200 8N1 DTR=1 RTS=0"
 $broadcast=Receive 1200;Log ('BROADCAST '+[BitConverter]::ToString($broadcast))
 $identity=[Text.Encoding]::ASCII.GetString($broadcast)
 if($identity -match '\x00MSN[0-9]{2}') {
  Ack ([byte[]](0,77,83,78,67,78));Log 'MSU2 handshake confirmed.'
 } else {
  # Closing a host handle does not immediately end firmware's online state.
  # Confirm a known dictionary entry before resuming an existing session.
  Send ([byte[]](0,0x30,0,1,0,0));$resume=Receive 1000
  Log ('RX resume check '+[BitConverter]::ToString($resume))
  if([BitConverter]::ToString($resume) -ne '00-30-00-01-00-4C') {throw 'No MSN broadcast or valid MSU2 dictionary response. Reconnect the screen.'}
  Log 'Existing MSU2 online session verified.'
 }
 if($Mode -ne 'Info'){Ack ([byte[]](2,3,10,0,0,0))}
 if($Mode -eq 'Pattern'){
  $halfW=[int][Math]::Floor($Width/2);$halfH=[int][Math]::Floor($Height/2)
  Fill 0 0 $halfW $halfH 0xF800
  Fill $halfW 0 ($Width-$halfW) $halfH 0x07E0
  Fill 0 $halfH $halfW ($Height-$halfH) 0x001F
  Fill $halfW $halfH ($Width-$halfW) ($Height-$halfH) 0xFFFF
  Log 'Pattern acknowledged. Check physical panel: red/green above blue/white.'
 }
 if($Mode -eq 'Image'){
  Add-Type -AssemblyName System.Drawing
  $source=[Drawing.Image]::FromFile((Resolve-Path -LiteralPath $ImagePath).Path)
  $bitmap=[Drawing.Bitmap]::new($Width,$Height)
  try {
   $graphics=[Drawing.Graphics]::FromImage($bitmap)
   try {$graphics.DrawImage($source,0,0,$Width,$Height)} finally {$graphics.Dispose()}
   $pixels=[byte[]]::new($Width*$Height*2);$offset=0
   for($y=0;$y -lt $Height;$y++){for($x=0;$x -lt $Width;$x++){
    $c=$bitmap.GetPixel($x,$y)
    # Widen channel bytes before shifting: PowerShell otherwise truncates
    # left shifts to byte width, losing the RGB565 red and green high bits.
    $value=(([int]$c.R -shr 3) -shl 11) -bor (([int]$c.G -shr 2) -shl 5) -bor ([int]$c.B -shr 3)
    $pixels[$offset]=[byte]($value -shr 8);$pixels[$offset+1]=[byte]($value -band 255);$offset+=2
   }}
   Window 0 0 $Width $Height;Ack ([byte[]](2,3,7,0,0,0))
   for($pos=0;$pos -lt $pixels.Length;$pos+=256){
    $count=[Math]::Min(256,$pixels.Length-$pos)
    $buffer=[byte[]]::new(384)
    for($i=0;$i -lt 64;$i++){
     $buffer[$i*6]=4;$buffer[$i*6+1]=[byte]$i
     for($j=0;$j -lt 4;$j++){
      $idx=$i*4+$j
      $buffer[$i*6+2+$j]=if($idx -lt $count){$pixels[$pos+$idx]}else{255}
     }
    }
    # Each block contains complete six-byte instructions and fits a USB packet.
    for($b=0;$b -lt 384;$b+=60){$n=[Math]::Min(60,384-$b);$serial.Write($buffer,$b,$n)}
    # Pixel page commits have no ACK on the tested version 01 firmware.
    Send ([byte[]](2,3,8,($count -shr 8),($count -band 255),0))
    Start-Sleep -Milliseconds 10
   }
   Send ([byte[]](8,9,0,0,0,0));$reply=Receive 3000
   Log ('RX ADC after image '+[BitConverter]::ToString($reply))
   if($reply.Length -lt 6 -or $reply[0] -ne 8 -or $reply[1] -ne 9){throw 'No valid ADC response after image transfer.'}
   Log ('Image sent; command parser responsive. RGB565 big endian, '+$pixels.Length+' pixel bytes. Check physical display.')
  } finally {$bitmap.Dispose();$source.Dispose()}
 }
 if($Mode -ne 'Info' -and $HoldSeconds -gt 0){
  Log "Keeping connection active for $HoldSeconds seconds."
  $hold=[Diagnostics.Stopwatch]::StartNew()
  while($hold.Elapsed.TotalSeconds -lt $HoldSeconds){
   $serial.Write([byte[]](8,9,0,0,0,0),0,6);$null=Receive 500
   Start-Sleep -Milliseconds 200
  }
 }
} finally {
 $serial.Dispose();$trace | Set-Content -LiteralPath $logPath -Encoding UTF8
 Write-Host ('Log: '+$logPath)
}
