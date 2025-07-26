#!/usr/bin/env pwsh

# greplog.ps1 - PowerShell equivalent of Linux grep for Unity Editor.log
# Usage: greplog <search_term>
# Example: greplog familyDagDebugContent

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$SearchTerm
)

# Path to Unity Editor.log file
$logPath = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"

# Check if the log file exists
if (-not (Test-Path $logPath)) {
    Write-Error "Editor.log not found at: $logPath"
    exit 1
}

# Search for the term in the log file (case-insensitive)
try {
    Get-Content $logPath | Where-Object { $_ -match $SearchTerm }
} catch {
    Write-Error "Error reading log file: $($_.Exception.Message)"
    exit 1
} 