$CurrentFolder = "./Current"
$WindowsFolder = "$CurrentFolder/Windows Desktop"
$LinuxFolder = "$CurrentFolder/Linux"
$macOSFolder = "$CurrentFolder/macOS"

# Make folder structure
if (Test-Path -Path $CurrentFolder) {
    Write-Host "Removing current build ..."
    Remove-Item -Force -Recurse -Path $CurrentFolder | Out-Null
    Write-Host ""
}

Write-Host "Re-Creating current folder structure ..."
New-Item -Force -ItemType Directory -Name $CurrentFolder | Out-Null
New-Item -Force -ItemType Directory -Name $WindowsFolder | Out-Null
New-Item -Force -ItemType Directory -Name $LinuxFolder | Out-Null
New-Item -Force -ItemType Directory -Name $macOSFolder | Out-Null
Write-Host ""

Write-Host "Exporting for: Windows Desktop"
godot_console.exe --headless --path '../' --export-release 'Windows Desktop' "Builds/$WindowsFolder/Game.exe"
Write-Host ""

Write-Host "Exporting for: Linux"
godot_console.exe --headless --path '../' --export-release 'Linux' "Builds/$LinuxFolder/Game.x86_64"
Write-Host ""

Write-Host "Exporting for: macOS"
godot_console.exe --headless --path '../' --export-release 'macOS' "Builds/$macOSFolder/Game.zip"
Write-Host ""
