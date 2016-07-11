<#
.SYNOPSIS
    Restores NuGet packages.
#>
Param(
    [Parameter()]
    [switch]$Force
)

Push-Location $PSScriptRoot
try {

    $toolsPath = "$PSScriptRoot\tools"

    # Restore VS solution dependencies
    & "$toolsPath\Restore-NuGetPackages.ps1" -Path "ADAL.NET.DesktopAndCoreCLR.sln"

    Write-Host "Successfully restored all dependencies" -ForegroundColor Yellow
}
catch {
    Write-Error "Aborting script due to error $($_.Exception.Message)."
    exit $lastexitcode
}
finally {
    Pop-Location
}
