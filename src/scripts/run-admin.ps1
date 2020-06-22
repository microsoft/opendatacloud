param (
    [string] $profileName = "PowerShell",
    [switch] $start = $false
)

# Use Windows Terminal (https://github.com/microsoft/terminal) to start
# Admin UI and Admin Web projects locally.

# Make sure your Azure identity information is added to KeyVault:
# docs/deployment-initialization.md


$odrUIPath = Join-Path $PSScriptRoot ../odr-ui -Resolve
$adminPath = Join-Path $PSScriptRoot ../Msr.Odr.WebAdminPortal -Resolve

$procArgs = @(
    "new-tab -p `"$profileName`" -d `"$odrUIPath`""
    "split-pane -p `"$profileName`" -d `"$adminPath`""
)

if($start) {
    $procArgs = @(
        $procArgs[0] + " npm.cmd run start:admin"
        $procArgs[1] + " dotnet.exe watch run --urls `"http://localhost:58784/`""
    )
}

$argList = $procArgs -join " ; "

Start-Process -FilePath "wt.exe" -ArgumentList $argList
