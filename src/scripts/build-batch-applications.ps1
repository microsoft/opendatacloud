
$root = Join-Path $PSScriptRoot ".."
$buildPath = Join-Path $PSScriptRoot "build"

if ( Test-Path -Path $buildPath -PathType Container ) {
    Remove-Item $buildPath -Force -Recurse
}
mkdir $buildPath | Out-Null

function buildProject([string] $name)
{
    Write-Host "Building $name ..."
    $projectPath = Join-Path $root "Msr.Odr.Batch.$name" -Resolve
    $publishPath = Join-Path $projectPath "bin\\release\\netcoreapp2.1\\win10-x64\\publish"
    $zipFile = Join-Path $buildPath "$name.zip"

    dotnet publish -nologo -c release --self-contained -r win10-x64 $projectPath
    if ( $? -eq $false ) { exit }

    $filesToZip = Join-Path $publishPath '*'
    Compress-Archive -Path $filesToZip -DestinationPath $zipFile
    if ( $? -eq $false ) { exit }
}

Write-Host "Working from $root"
Write-Host "Building to $buildPath"

if(!(Test-Path $buildPath)) {
    New-Item -ItemType Directory -Path $buildPath | Out-Null
    Write-Host "Created $buildPath"
}
Get-ChildItem -Path $buildPath | Remove-Item

buildProject "CompressDatasetApp"
buildProject "ImportDatasetApp"

Write-Host "Finished."
Get-ChildItem $buildPath
