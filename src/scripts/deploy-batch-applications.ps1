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

    Get-AzBatchApplication `
        -AccountName $batchAcct.AccountName `
        -ResourceGroupName $batchAcct.ResourceGroupName |
        ForEach-Object {
            $applicationId = $_.Id
            $version = $_.DefaultVersion

            $package = Get-AzBatchApplicationPackage `
                -AccountName $batchAcct.AccountName `
                -ResourceGroupName $batchAcct.ResourceGroupName `
                -ApplicationId $applicationId `
                -ApplicationVersion $version

            $package |
                Select-Object Id,LastActivationTime,Version,State |
                Format-Table

            if($update) {

                Write-Host "Updating application ..."

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
                }
        }
}
