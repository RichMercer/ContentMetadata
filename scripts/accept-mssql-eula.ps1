<#
.SYNOPSIS
    Records local SQL Server EULA acceptance for the Community stack

.USAGE
    .\accept-mssql-eula.ps1 [-Yes]
#>

param(
    [switch]$Yes
)

$scriptFile = $MyInvocation.MyCommand.Definition
$resolvedPath = (Resolve-Path -LiteralPath $scriptFile).Path
$scriptDir = Split-Path -Parent $resolvedPath
$eulaEnvFile = Join-Path $scriptDir "..\.devcontainer\mssql-eula.env"

Write-Host "SQL Server for this stack is pulled from Microsoft's container registry."
Write-Host "Your use of SQL Server is governed by Microsoft's End-User License Agreement."
Write-Host ""
Write-Host "Accepting here sets a local environmental variable (ACCEPT_EULA=Y)"
Write-Host "confirming you have a valid license for SQL Server. SQL Server runs in"
Write-Host "the free Developer edition (MSSQL_PID=Developer) by default."
Write-Host ""
Write-Host "For more information:"
Write-Host "https://hub.docker.com/r/microsoft/mssql-server#environment-variables"
Write-Host "https://go.microsoft.com/fwlink/?linkid=857698"

if (-not $Yes) {
    if (-not [Environment]::UserInteractive -or $Host.Name -match 'ServerRemoteHost') {
        Write-Error "'-Yes' is required when no interactive terminal is available."
        exit 1
    }

    $response = Read-Host "Type 'yes' to accept and continue"
    if ($response -cne 'yes') {
        Write-Host "SQL Server End-User License Agreement acceptance was not recorded."
        exit 1
    }
}

$eulaDir = Split-Path -Parent $eulaEnvFile
New-Item -ItemType Directory -Path $eulaDir -Force | Out-Null
Set-Content -LiteralPath $eulaEnvFile -Value 'ACCEPT_EULA=Y'
Write-Host "Created $eulaEnvFile"
