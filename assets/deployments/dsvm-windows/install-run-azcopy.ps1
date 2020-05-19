# Installs AzCopy and copies dataset from Azure Storage.

Param (
    [Parameter(Mandatory=$true)]
    [string]$url,
    [Parameter(Mandatory=$true)]
    [string]$path
)

# Install AzCopy
$azCopy = "${env:ProgramFiles(x86)}\Microsoft SDKs\Azure\AzCopy\AzCopy.exe"
if (Test-Path $azCopy) {
    'AzCopy has already been installed.'
} else {
    'Installing AzCopy ...'
    $msiFile = "${env:Temp}\MicrosoftAzureStorageTools.msi"
    Invoke-WebRequest http://aka.ms/downloadazcopy -outfile $msiFile
    Start-Process msiexec.exe -Wait -ArgumentList "/I `"$msiFile`" /quiet"
}

# Copy the dataset
& $azCopy "/Source:$url" "/Dest:$path" /s /y /v
