# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param (
    [string] $name,
    [switch] $update = $false
)

$ErrorActionPreference = "Stop"

$buildPath = Join-Path $PSScriptRoot "build"

$batchJobName = 'DatasetJob'

Write-Host "Deploying these applications:"
Get-ChildItem -Path $buildPath |
    Select-Object LastWriteTime, Length, Name |
    Format-Table

if([string]::IsNullOrWhiteSpace($name)) {
    Get-AzBatchAccount |
        Select-Object TaskTenantUrl, AccountName |
        Format-Table
    Write-Host "Specify the batch account using the -name parameter."
} else {

    $batchAcct = Get-AzBatchAccount -AccountName $name
    $batchJob = Get-AzBatchJob -BatchContext $batchAcct -Id $batchJobName

    $batchJob |
        Select-Object Id, Url |
        Format-Table

    # Get-AzBatchApplication `
    #     -AccountName $batchAcct.AccountName `
    #     -ResourceGroupName $batchAcct.ResourceGroupName |
    #     ForEach-Object {

    #         $applicationId = $_.Name
    #         $version = $_.DefaultVersion

    #         Get-AzBatchApplicationPackage `
    #             -AccountName $batchAcct.AccountName `
    #             -ResourceGroupName $batchAcct.ResourceGroupName `
    #             -ApplicationId $applicationId `
    #             -ApplicationVersion $version |
    #             Format-List
    #     }

    if($update) {

        Write-Host "Updating applications ..."

        Get-ChildItem -Path $buildPath |
            Select-Object -ExpandProperty BaseName |
            ForEach-Object {
                $applicationId = $_
                $version = '1.0'
                $filePath = Join-Path $buildPath "$($applicationId).zip"
                New-AzBatchApplicationPackage `
                    -AccountName $batchAcct.AccountName `
                    -ResourceGroupName $batchAcct.ResourceGroupName `
                    -ApplicationId $applicationId `
                    -ApplicationVersion $version `
                    -FilePath $filePath `
                    -Format "zip" |
                    Select-Object Id,LastActivationTime,Version,State |
                    Format-Table
                Set-AzBatchApplication `
                    -AccountName $batchAcct.AccountName `
                    -ResourceGroupName $batchAcct.ResourceGroupName `
                    -ApplicationId $applicationId `
                    -DefaultVersion "1.0" `
                    -AllowUpdates $true
            }
    }
}
