param (
    [string] $wixPath = 'C:\Tools\Wix'
)

Set-Location $PSScriptRoot

if ( Test-Path -Path .\build -PathType Container ) {
    Remove-Item .\build -Force -Recurse
}
New-Item -Path . -Name build -ItemType directory | Out-Null

dotnet build -c release .\DatasetUtil\DatasetUtil.csproj
if ( $? -eq $false ) { exit 1 }

dotnet publish -c release --self-contained -r win10-x64 .\DatasetUtil\DatasetUtil.csproj
if ( $? -eq $false ) { exit 1 }

& "$wixPath\heat.exe" dir .\DatasetUtil\bin\release\netcoreapp2.1\win10-x64\publish -dr INSTALLDIR -cg DatasetUtilGroup -o ./build/DatasetUtilGroup.wxs -srd -scom -sfrag -sreg -ag -var var.UtilSource
if ( $? -eq $false ) { exit 1 }

& "$wixPath\candle" DatasetUtil.wxs .\build\DatasetUtilGroup.wxs -arch x64 -o .\build\ -dUtilSource=.\DatasetUtil\bin\release\netcoreapp2.1\win10-x64\publish
if ( $? -eq $false ) { exit 1 }

& "$wixPath\light" .\build\DatasetUtil.wixobj .\build\DatasetUtilGroup.wixobj -ext WixUIExtension -o .\build\DatasetUtil.msi
if ( $? -eq $false ) { exit 1 }

Write-Output "Built DatasetUtil.msi"
Get-ChildItem .\build
