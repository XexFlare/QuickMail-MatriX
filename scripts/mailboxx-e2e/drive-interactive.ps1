param(
    [Parameter(Mandatory)] [string]$Exe,
    [Parameter(Mandatory)] [string]$Profile,
    [Parameter(Mandatory)] [string]$OutDir
)
# Drives the real MailBoXX window with Windows UI Automation against the local GreenMail profile.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing, System.Windows.Forms
Add-Type @"
using System; using System.Runtime.InteropServices;
public static class Win { [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c); }
"@
New-Item -ItemType Directory -Force $OutDir | Out-Null
$AE = [System.Windows.Automation.AutomationElement]
$TS = [System.Windows.Automation.TreeScope]

function Shot($name) {
    $r = $script:win.Current.BoundingRectangle
    $bmp = New-Object System.Drawing.Bitmap ([int]$r.Width), ([int]$r.Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen([int]$r.X, [int]$r.Y, 0, 0, $bmp.Size)
    $bmp.Save((Join-Path $OutDir "$name.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    "shot: $name"
}
function Front { [Win]::ShowWindow([IntPtr]$script:win.Current.NativeWindowHandle, 9) | Out-Null
                 [Win]::SetForegroundWindow([IntPtr]$script:win.Current.NativeWindowHandle) | Out-Null; Start-Sleep -Milliseconds 400 }
function Find($root, $prop, $value) {
    $c = New-Object System.Windows.Automation.PropertyCondition ($prop, $value)
    $root.FindFirst($TS::Descendants, $c)
}
function Items() {
    $list = Find $script:win $AE::AutomationIdProperty 'MessageList'
    $c = New-Object System.Windows.Automation.PropertyCondition ($AE::ControlTypeProperty, [System.Windows.Automation.ControlType]::ListItem)
    @($list.FindAll($TS::Children, $c))
}
function Open($match) {
    $item = Items | Where-Object { $_.Current.Name -like "*$match*" } | Select-Object -First 1
    if (-not $item) { throw "no message row matching '$match'" }
    $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    $item.SetFocus(); Front
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    Start-Sleep -Seconds 4
}

$proc = Start-Process -FilePath $Exe -ArgumentList @('--profileDir', "`"$Profile`"") -PassThru
$deadline = (Get-Date).AddSeconds(40)
do { Start-Sleep 1; $proc.Refresh() } until ($proc.MainWindowHandle -ne 0 -or (Get-Date) -gt $deadline)
$script:win = $AE::FromHandle($proc.MainWindowHandle)
"window: $($win.Current.Name)"
Front

# Wait for the sync to bring the four GreenMail messages into the list.
$deadline = (Get-Date).AddSeconds(60)
do { Start-Sleep 2; $n = (Items).Count } until ($n -ge 4 -or (Get-Date) -gt $deadline)
"rows: $n"
(Items) | ForEach-Object { "  row: $($_.Current.Name)" }
Shot '01-synced-list'

Open 'safari';  Shot '02-reading-html'
Open 'Invoice'; Shot '03-reading-attachment'
"rows after reading:"; (Items) | ForEach-Object { "  row: $($_.Current.Name)" }
Shot '04-list-after-reading'

# Delete one message from the list with the Delete key (moves to Trash on the server).
$photo = Items | Where-Object { $_.Current.Name -like '*Photo walk*' } | Select-Object -First 1
$photo.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
$photo.SetFocus(); Front
[System.Windows.Forms.SendKeys]::SendWait('{DELETE}')
Start-Sleep -Seconds 4
"rows after delete:"; (Items) | ForEach-Object { "  row: $($_.Current.Name)" }
Shot '05-after-delete'

$proc.CloseMainWindow() | Out-Null
if (-not $proc.WaitForExit(15000)) { $proc.Kill() }
"exited"
