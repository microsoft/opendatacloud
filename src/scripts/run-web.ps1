param (
    [string] $profileName = "PowerShell",
    [switch] $start = $false
)

# Use Windows Terminal (https://github.com/microsoft/terminal) to start
# Web UI, Web Server and Web API projects locally.

# Make sure your Azure identity information is added to KeyVault:
# docs/deployment-initialization.md


$odrUIPath = Join-Path $PSScriptRoot ../odr-ui -Resolve
$webPath = Join-Path $PSScriptRoot ../Msr.Odr.Web -Resolve
$apiPath = Join-Path $PSScriptRoot ../Msr.Odr.WebApi -Resolve

$procArgs = @(
    "new-tab -p `"$profileName`" -d `"$odrUIPath`""
    "split-pane -p `"$profileName`" -d `"$webPath`""
    "split-pane -p `"$profileName`" -d `"$apiPath`""
)

if($start) {
    $procArgs = @(
        $procArgs[0] + " npm.cmd run start:web"
        $procArgs[1] + " dotnet.exe watch run --urls `"http://localhost:53048/`""
        $procArgs[2] + " dotnet.exe watch run --urls `"http://localhost:31812/`""
    )
}

$argList = $procArgs -join " ; "

Start-Process -FilePath "wt.exe" -ArgumentList $argList
