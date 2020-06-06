param (
    [string] $StorageAccountName = '',
    [string] $StorageGroupName = '',
    [string] $StorageContainerName = 'azure-arm-deploy',
    [Parameter(Mandatory=$true)]
    [string] $ResourceGroupName,
    [switch] $ShowDebug
)

$ErrorActionPreference = 'Stop'
Clear-Host

Try
{
    # Get current Azure Subscription context
    # (assumes that Connect-AzAccount has already been called)
    $ctx = Get-AzContext
    Write-Output "Using $($ctx.Name)"

    # Find the storage account to use for storing templates
    if($StorageAccountName -eq '' -or $StorageGroupName -eq '') {
        $storage = (Get-AzStorageAccount |
            Where-Object { $_.tags["ms-resource-usage"] -eq "azure-cloud-shell" } |
            Select-Object -First 1)[0]
    } else {
        $storage = (Get-AzStorageAccount -ResourceGroupName $StorageGroupName -Name $StorageAccountName)[0]
    }
    if($null -eq $storage) {
        throw "Could not find storage account."
    }
    Write-Output "Storing $($storage.StorageAccountName)/$($StorageContainerName)"

    # Create the blob container
    $container = $storage | Get-AzStorageContainer -Name $StorageContainerName -ErrorAction Ignore
    if(-not $container) {
        $container = $storage | New-AzStorageContainer -Name $StorageContainerName
    }

    # Copy the ARM templates to the blob container
    Get-ChildItem -Filter *.json -Path "Templates" -Recurse |
        Where-Object { $_.Name -ne "azuredeploy.parameters.json" } |
        ForEach-Object {
            $fullName = $_.FullName
            $relativePath = (Resolve-Path -Path $fullName -Relative).Substring(12) -replace '[\\]', '/'
            $blob = $container | Get-AzStorageBlob -Blob $relativePath -ErrorAction Ignore
            if(-not $blob) {
                $status = $container | Set-AzStorageBlobContent -File $fullName -Blob $relativePath
            } else {
                $status = $blob | Set-AzStorageBlobContent -File $fullName -Force
            }
            Write-Output "  => $($status.Name)"
        }

    # Get SAS token for the container
    $sasToken = $container | New-AzStorageContainerSASToken -Permission r -ExpiryTime (Get-Date).AddHours(4)
    $blobUri = $container.CloudBlobContainer.GetBlobReference('azuredeploy.json').Uri
    $blobUriSas = "$($blobUri)$($sasToken)"

    # Update the parameters
    $params = @{
        _artifactsLocation = "$($container.CloudBlobContainer.Uri)/"
        _artifactsLocationSasToken = "$($sasToken)"
    }
    $current = Get-ChildItem -Path "Templates/azuredeploy.parameters.json" |
        Get-Content |
        ConvertFrom-Json
    $current.parameters.psobject.Properties |
        ForEach-Object {
            $params.Add($_.Name, $_.Value.Value)
        }

    # Test the ARM templates
    Write-Output "Testing $($blobUri)"
    Test-AzResourceGroupDeployment `
        -Mode Incremental `
        -ResourceGroupName $ResourceGroupName `
        -TemplateUri $blobUriSas `
        -TemplateParameterObject $params `
        -SkipTemplateParameterPrompt `
        -Verbose `
        -Debug:$ShowDebug
}
Catch
{
    Write-Error $_.Exception
}
