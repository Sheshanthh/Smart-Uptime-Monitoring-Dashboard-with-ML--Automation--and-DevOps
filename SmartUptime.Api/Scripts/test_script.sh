#!/bin/bash

# Test script for Smart Uptime Monitor
# This script logs a test message to verify script execution

LOG_FILE="/var/log/smart-uptime/script-execution.log"
TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')

# Create log directory if it doesn't exist
mkdir -p /var/log/smart-uptime

echo "[$TIMESTAMP] Test script executed successfully" >> "$LOG_FILE"
echo "[$TIMESTAMP] Script arguments: $1" >> "$LOG_FILE"
echo "[$TIMESTAMP] Script executed from: $(pwd)" >> "$LOG_FILE"

# Simulate some work
sleep 2

echo "[$TIMESTAMP] Test script completed successfully" >> "$LOG_FILE"

exit 0 