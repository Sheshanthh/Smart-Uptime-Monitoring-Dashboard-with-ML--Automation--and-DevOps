# Test Script for Smart Uptime Monitoring
# This script simulates a service restart operation for testing

param(
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "TestService"
)

$LogFile = "C:\logs\smart-uptime\test-script.log"
$Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

# Create log directory if it doesn't exist
$LogDir = Split-Path $LogFile -Parent
if (!(Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
}

# Function to write to log
function Write-Log {
    param([string]$Message)
    $LogEntry = "[$Timestamp] $Message"
    Add-Content -Path $LogFile -Value $LogEntry
    Write-Host $LogEntry
}

Write-Log "Starting test script execution for service: $ServiceName"

# Simulate checking if service exists
Write-Log "Checking if service $ServiceName exists..."

# Simulate service status check
$ServiceStatus = "Running"
Write-Log "Current status of $ServiceName: $ServiceStatus"

# Simulate stopping service
Write-Log "Simulating stop of service $ServiceName..."
Start-Sleep -Seconds 1
Write-Log "Service $ServiceName stopped successfully"

# Wait a moment
Start-Sleep -Seconds 1

# Simulate starting service
Write-Log "Simulating start of service $ServiceName..."
Start-Sleep -Seconds 1
Write-Log "Service $ServiceName started successfully"

# Check final status
Start-Sleep -Seconds 1
$FinalStatus = "Running"
Write-Log "Final status of $ServiceName: $FinalStatus"

Write-Log "SUCCESS: Test script completed successfully for $ServiceName"
exit 0 