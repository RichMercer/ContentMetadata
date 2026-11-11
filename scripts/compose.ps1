<#
.SYNOPSIS
    Wrapper for Community compose projects

.USAGE
    .\script.ps1 <command> [options]
#>

$scriptFile   = $MyInvocation.MyCommand.Definition
$resolvedPath = (Resolve-Path -LiteralPath $scriptFile).Path
$scriptDir    = Split-Path -Parent $resolvedPath
$scriptName   = Split-Path -Leaf $resolvedPath
$eulaScript = Join-Path $scriptDir "accept-mssql-eula.ps1"
$eulaEnvFile = Join-Path $scriptDir "..\.devcontainer\mssql-eula.env"

function Show-Help {
    param(
        [string]$Name
    )
    $helpText = @"
Usage: $Name <command> [options]

Commands:
  up       Start the containers (passes -d to detach)
  down     Stop and remove the containers
  ps       List containers
  logs     View container logs
  exec     Execute a command in a running container
  Any other docker compose command

Options:
  -h       Show this help message

Examples:
  $Name up
  $Name down
"@
    Write-Host $helpText
}

function Test-IsInteractive {
    return [Environment]::UserInteractive -and $Host.Name -notmatch 'ServerRemoteHost'
}

function Ensure-MsSqlEula {
    if (Test-Path -LiteralPath $eulaEnvFile) {
        return
    }

    if (Test-IsInteractive) {
		Write-Host "SQL Server for this stack is pulled from Microsoft's container registry."
		Write-Host "Your use of SQL Server is governed by Microsoft's End-User License Agreement."
        Write-Host ""
        Write-Host "For more information:"
        Write-Host "https://hub.docker.com/r/microsoft/mssql-server#environment-variables"
        Write-Host "https://go.microsoft.com/fwlink/?linkid=857698"
        Write-Host ""
        & $eulaScript
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
        return
    }

    Write-Error "SQL Server End-User License Agreement acceptance is required before starting the stack. Run '$eulaScript' once to create '$eulaEnvFile' and confirm your acceptance."
    exit 1
}

if ($args.Count -eq 0) {
    Write-Error "Error: No command specified."
    Show-Help -Name $scriptName
    exit 1
}

$cmd = $args[0]
if ($cmd -eq '-h' -or $cmd -eq '--help') {
    Show-Help -Name $scriptName
    exit 0
}

if ($args.Count -gt 1) {
    $remainingArgs = $args[1..($args.Count - 1)]
} else {
    $remainingArgs = @()
}

if ($cmd -eq 'up') {
    $extraFlags = @('-d')
} else {
    $extraFlags = @()
}

if ($cmd -in @('up', 'start', 'restart')) {
    Ensure-MsSqlEula
}

$preArgs = @()

# Detect containerization host
if (Get-Command docker -ErrorAction SilentlyContinue) {
    $containerHostCmd = "docker"
	$preArgs += "compose"
} elseif (Get-Command podman -ErrorAction SilentlyContinue) {
    $containerHostCmd = "podman"
	$preArgs += "compose"
} else {
	$wslDockerCheck = & wsl.exe docker compose version 2>&1
	if ($LASTEXITCODE -eq 0) {
		$containerHostCmd = "wsl"
		$preArgs += "docker"
		$preArgs += "compose"
	} else {
		Write-Error "Error: Neither Docker nor Podman nor WSL with Docker are available."
		exit 1
	}
}

if ($containerHostCmd -eq "wsl") {
	# If it will run with WSL + Docker, path separator will be "/",
	# as it will ultimately be run within Linux
	$pathsep = "/"
	# adjust script dir
    $drive = $scriptDir.Substring(0,1).ToLower()
    $rest  = $scriptDir.Substring(2).TrimStart('\') -replace '\\','/'
	$scriptDir = "/mnt/$drive/$rest"
} else {
	# Otherwise, path separator should match the current host OS
	$pathsep = [System.IO.Path]::DirectorySeparatorChar
}

$commonArgs  = @(
    '-f', (($scriptDir, '..', '.devcontainer', 'compose.yml') -join $pathsep),
    $cmd
)

$allArgs = $preArgs + $commonArgs + $extraFlags
if ($remainingArgs.Count -gt 0) {
	$allArgs += $remainingArgs
}

Write-Host "Executing: $containerHostCmd $($allArgs -join ' ')"
& $containerHostCmd @allArgs
