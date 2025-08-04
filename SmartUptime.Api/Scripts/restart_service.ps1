# Emergency Service Restart Script (PowerShell)
# This script restarts a service when downtime is detected

param(
    [Parameter(Mandatory=$true)]
    [string]$ServiceName
)

$LogFile = "C:\logs\smart-uptime\script-execution.log"
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

Write-Log "Starting emergency restart for service: $ServiceName"

# Check if service exists
try {
    $Service = Get-Service -Name $ServiceName -ErrorAction Stop
    Write-Log "Service $ServiceName found"
} catch {
    Write-Log "ERROR: Service $ServiceName not found"
    exit 1
}

# Check current status
$CurrentStatus = $Service.Status
Write-Log "Current status of $ServiceName: $CurrentStatus"

# Stop the service
Write-Log "Stopping service $ServiceName..."
try {
    Stop-Service -Name $ServiceName -Force -ErrorAction Stop
    Write-Log "Service $ServiceName stopped successfully"
} catch {
    Write-Log "WARNING: Failed to stop service $ServiceName"
}

# Wait a moment
Start-Sleep -Seconds 2

# Start the service
Write-Log "Starting service $ServiceName..."
try {
    Start-Service -Name $ServiceName -ErrorAction Stop
    Write-Log "Service $ServiceName started successfully"
} catch {
    Write-Log "ERROR: Failed to start service $ServiceName"
    exit 1
}

# Check final status
Start-Sleep -Seconds 3
$FinalStatus = (Get-Service -Name $ServiceName).Status
Write-Log "Final status of $ServiceName: $FinalStatus"

if ($FinalStatus -eq "Running") {
    Write-Log "SUCCESS: Service $ServiceName restarted successfully"
    exit 0
} else {
    Write-Log "ERROR: Service $ServiceName failed to start properly"
    exit 1
} 