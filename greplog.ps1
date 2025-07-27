#!/usr/bin/env pwsh

# greplog.ps1 - PowerShell equivalent of Linux grep for Unity Editor.log
# Usage: greplog <search_term>
# Example: greplog familyDagDebugContent

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$SearchTerm,
    
    [Parameter(Mandatory=$false)]
    [int]$Start,
    
    [Parameter(Mandatory=$false)]
    [int]$Count
)

# Path to Unity Editor.log file
$logPath = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"

# Check if the log file exists
if (-not (Test-Path $logPath)) {
    Write-Error "Editor.log not found at: $logPath"
    exit 1
}

# Search for the term in the log file (case-insensitive) with line numbers
try {
    $lineNumber = 0
    $outputCount = 0
    
    Get-Content $logPath | ForEach-Object { 
        $lineNumber++
        
        # Skip if we haven't reached the start line yet
        if ($Start -and $lineNumber -lt $Start) {
            return
        }
        
        # Stop if we've reached the count limit
        if ($Count -and $outputCount -ge $Count) {
            return
        }
        
        if ($_ -match $SearchTerm) {
            Write-Output "$lineNumber`: $_"
            $outputCount++
        }
    }
} catch {
    Write-Error "Error reading log file: $($_.Exception.Message)"
    exit 1
} 