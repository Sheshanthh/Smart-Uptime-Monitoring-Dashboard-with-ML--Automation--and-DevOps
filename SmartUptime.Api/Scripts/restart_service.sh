#!/bin/bash

# Emergency Service Restart Script
# This script restarts a service when downtime is detected

SERVICE_NAME="$1"
LOG_FILE="/var/log/smart-uptime/script-execution.log"
TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')

# Create log directory if it doesn't exist
mkdir -p /var/log/smart-uptime

echo "[$TIMESTAMP] Starting emergency restart for service: $SERVICE_NAME" >> "$LOG_FILE"

# Check if service exists
if ! systemctl list-unit-files | grep -q "$SERVICE_NAME"; then
    echo "[$TIMESTAMP] ERROR: Service $SERVICE_NAME not found" >> "$LOG_FILE"
    exit 1
fi

# Check current status
SERVICE_STATUS=$(systemctl is-active "$SERVICE_NAME")
echo "[$TIMESTAMP] Current status of $SERVICE_NAME: $SERVICE_STATUS" >> "$LOG_FILE"

# Stop the service
echo "[$TIMESTAMP] Stopping service $SERVICE_NAME..." >> "$LOG_FILE"
if systemctl stop "$SERVICE_NAME"; then
    echo "[$TIMESTAMP] Service $SERVICE_NAME stopped successfully" >> "$LOG_FILE"
else
    echo "[$TIMESTAMP] WARNING: Failed to stop service $SERVICE_NAME" >> "$LOG_FILE"
fi

# Wait a moment
sleep 2

# Start the service
echo "[$TIMESTAMP] Starting service $SERVICE_NAME..." >> "$LOG_FILE"
if systemctl start "$SERVICE_NAME"; then
    echo "[$TIMESTAMP] Service $SERVICE_NAME started successfully" >> "$LOG_FILE"
else
    echo "[$TIMESTAMP] ERROR: Failed to start service $SERVICE_NAME" >> "$LOG_FILE"
    exit 1
fi

# Check final status
sleep 3
FINAL_STATUS=$(systemctl is-active "$SERVICE_NAME")
echo "[$TIMESTAMP] Final status of $SERVICE_NAME: $FINAL_STATUS" >> "$LOG_FILE"

if [ "$FINAL_STATUS" = "active" ]; then
    echo "[$TIMESTAMP] SUCCESS: Service $SERVICE_NAME restarted successfully" >> "$LOG_FILE"
    exit 0
else
    echo "[$TIMESTAMP] ERROR: Service $SERVICE_NAME failed to start properly" >> "$LOG_FILE"
    exit 1
fi 