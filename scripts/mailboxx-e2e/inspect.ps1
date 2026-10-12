param([Parameter(Mandatory)] [string]$Exe, [Parameter(Mandatory)] [string]$Profile, [Parameter(Mandatory)] [string]$OutPng, [string]$SelectFolder = '')
# Non-intrusive inspection: no keystrokes, no mouse, no screen capture. The window is rendered
# with PrintWindow (its own contents only) and read through UI Automation.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing
Add-Type @"
using System; using System.Runtime.InteropServices;
public static class PW { [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  public struct RECT { public int L, T, R, B; } }
"@
$AE = [System.Windows.Automation.AutomationElement]; $TS = [System.Windows.Automation.TreeScope]
$proc = Start-Process -FilePath $Exe -ArgumentList @('--profileDir', "`"$Profile`"") -PassThru
$deadline = (Get-Date).AddSeconds(40)
do { Start-Sleep 1; $proc.Refresh() } until ($proc.MainWindowHandle -ne 0 -or (Get-Date) -gt $deadline)
Start-Sleep -Seconds 12
$h = $proc.MainWindowHandle; $win = $AE::FromHandle($h)
"window: $($win.Current.Name)  rect: $($win.Current.BoundingRectangle)"
if ($SelectFolder) {
    $tree = $win.FindFirst($TS::Descendants, (New-Object System.Windows.Automation.PropertyCondition ($AE::AutomationIdProperty, 'FolderList')))
    $items = $tree.FindAll($TS::Descendants, (New-Object System.Windows.Automation.PropertyCondition ($AE::ControlTypeProperty, [System.Windows.Automation.ControlType]::TreeItem)))
    $target = @($items) | Where-Object { $_.Current.Name -like "$SelectFolder*" } | Select-Object -Last 1
    "selecting folder: $($target.Current.Name)"
    $target.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Seconds 4
}
$list = $win.FindFirst($TS::Descendants, (New-Object System.Windows.Automation.PropertyCondition ($AE::AutomationIdProperty, 'MessageList')))
"list rect: $($list.Current.BoundingRectangle)  offscreen: $($list.Current.IsOffscreen)"
$walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
$c = $walker.GetFirstChild($list); $i = 0
while ($c -ne $null -and $i -lt 20) { "  child: [$($c.Current.ControlType.ProgrammaticName)] $($c.Current.Name)"; $c = $walker.GetNextSibling($c); $i++ }
$r = New-Object PW+RECT; [PW]::GetWindowRect($h, [ref]$r) | Out-Null
$bmp = New-Object System.Drawing.Bitmap ($r.R - $r.L), ($r.B - $r.T)
$g = [System.Drawing.Graphics]::FromImage($bmp); $dc = $g.GetHdc()
[PW]::PrintWindow($h, $dc, 2) | Out-Null   # PW_RENDERFULLCONTENT
$g.ReleaseHdc($dc); $bmp.Save($OutPng, [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
"saved window image"
$proc.CloseMainWindow() | Out-Null
if (-not $proc.WaitForExit(15000)) { $proc.Kill() }
