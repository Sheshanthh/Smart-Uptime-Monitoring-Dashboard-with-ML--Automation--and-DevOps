#!/usr/bin/env python3
"""
Emergency Service Restart Script (Python)
This script restarts a service when downtime is detected
"""

import sys
import subprocess
import time
import logging
import os
from datetime import datetime
from pathlib import Path

def setup_logging():
    """Setup logging configuration"""
    log_dir = Path("/var/log/smart-uptime")
    log_dir.mkdir(parents=True, exist_ok=True)
    
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(levelname)s - %(message)s',
        handlers=[
            logging.FileHandler(log_dir / "script-execution.log"),
            logging.StreamHandler()
        ]
    )
    return logging.getLogger(__name__)

def run_command(command, check=True):
    """Run a shell command and return the result"""
    try:
        result = subprocess.run(
            command, 
            shell=True, 
            capture_output=True, 
            text=True, 
            check=check
        )
        return result.returncode, result.stdout, result.stderr
    except subprocess.CalledProcessError as e:
        return e.returncode, e.stdout, e.stderr

def check_service_exists(service_name):
    """Check if the service exists"""
    returncode, stdout, stderr = run_command(f"systemctl list-unit-files | grep {service_name}", check=False)
    return returncode == 0

def get_service_status(service_name):
    """Get the current status of the service"""
    returncode, stdout, stderr = run_command(f"systemctl is-active {service_name}", check=False)
    if returncode == 0:
        return stdout.strip()
    return "unknown"

def restart_service(service_name):
    """Restart the specified service"""
    logger = setup_logging()
    
    logger.info(f"Starting emergency restart for service: {service_name}")
    
    # Check if service exists
    if not check_service_exists(service_name):
        logger.error(f"Service {service_name} not found")
        return False
    
    # Check current status
    current_status = get_service_status(service_name)
    logger.info(f"Current status of {service_name}: {current_status}")
    
    # Stop the service
    logger.info(f"Stopping service {service_name}...")
    returncode, stdout, stderr = run_command(f"systemctl stop {service_name}", check=False)
    if returncode == 0:
        logger.info(f"Service {service_name} stopped successfully")
    else:
        logger.warning(f"Failed to stop service {service_name}: {stderr}")
    
    # Wait a moment
    time.sleep(2)
    
    # Start the service
    logger.info(f"Starting service {service_name}...")
    returncode, stdout, stderr = run_command(f"systemctl start {service_name}", check=False)
    if returncode == 0:
        logger.info(f"Service {service_name} started successfully")
    else:
        logger.error(f"Failed to start service {service_name}: {stderr}")
        return False
    
    # Check final status
    time.sleep(3)
    final_status = get_service_status(service_name)
    logger.info(f"Final status of {service_name}: {final_status}")
    
    if final_status == "active":
        logger.info(f"SUCCESS: Service {service_name} restarted successfully")
        return True
    else:
        logger.error(f"Service {service_name} failed to start properly")
        return False

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python restart_service.py <service_name>")
        sys.exit(1)
    
    service_name = sys.argv[1]
    success = restart_service(service_name)
    sys.exit(0 if success else 1) 