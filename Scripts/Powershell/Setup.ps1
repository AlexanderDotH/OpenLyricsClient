$WorkingDirectory = $PSScriptRoot + "../../../OpenLyricsClient.UI/bin/Debug/net7.0"
$BinaryPath = $PSScriptRoot + "../../../Binaries"

$UiProjectPath = $PSScriptRoot + "../../../OpenLyricsClient.UI/OpenLyricsClient.UI.csproj"

$AuthProjectPath = $PSScriptRoot + "../../../OpenLyricsClient.Auth/OpenLyricsClient.Auth.csproj"
$AuthFilePath = $WorkingDirectory + "/Authentication" 

$SharedProjectPath = $PSScriptRoot + "../../../OpenLyricsClient.Shared/OpenLyricsClient.Shared.csproj"

$TempPath = $PSScriptRoot + "../../../Temp"
$TempBin = $TempPath + "/Binaries"
$TempFile = $TempPath + "/Binaries.zip"

if (!(Test-Path -path $WorkingDirectory)) { New-Item $WorkingDirectory -Type Directory }

if (!(Test-Path -path $BinaryPath)) {

    Write-Output "----------------------------------------------------"
    Write-Output "Downloading necessary binaries"
    Write-Output "----------------------------------------------------"

    New-Item $BinaryPath -Type Directory
    New-Item $TempPath -Type Directory

    Invoke-WebRequest -Uri "https://alexh.space/files/Binaries.zip" -OutFile $TempFile

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($TempFile, $TempPath)

    Get-ChildItem -Path $TempBin -Recurse | Move-Item -Destination $BinaryPath

    Remove-Item -Path $TempPath -Recurse -Force
}

if ((Test-Path -path $BinaryPath)) {
    Get-ChildItem -Path $BinaryPath -Directory | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $WorkingDirectory -Recurse -Force
    }
}

if ((Test-Path -path $AuthProjectPath) -and !(Test-Path -path $AuthFilePath)) {

    Write-Output "----------------------------------------------------"
    Write-Output "Building Webview"
    Write-Output "----------------------------------------------------"

    Start-Process -FilePath "dotnet" -ArgumentList "publish `"$SharedProjectPath`" --configuration Release --output `"$AuthFilePath`"" -NoNewWindow -Wait
    Start-Process -FilePath "dotnet" -ArgumentList "publish `"$AuthProjectPath`" --configuration Release --output `"$AuthFilePath`"" -NoNewWindow -Wait
}

Write-Output "----------------------------------------------------"
Write-Output "You are ready to go"
Write-Output "----------------------------------------------------"