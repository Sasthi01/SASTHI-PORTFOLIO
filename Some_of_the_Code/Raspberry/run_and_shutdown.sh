#!/bin/bash
set -o pipefail

cd /home/soilmonitor/IoT_project_integrated

LOG_FILE=$(mktemp)

# Run the monitoring program and capture its output
PYTHONUNBUFFERED=1 ./start_monitor.sh 2>&1 | tee "$LOG_FILE"
result=${PIPESTATUS[0]}

# Shut down only after a successful six-reading session
if [ "$result" -eq 0 ] && grep -Eq "Session ended\. Readings collected: 6\." "$LOG_FILE"; then
    echo "Full monitoring session completed."
    echo "Automatically shutting down Raspberry Pi..."
    rm -f "$LOG_FILE"
    sudo -n /usr/sbin/shutdown -h now
else
    echo "Session incomplete or interrupted. Automatic shutdown cancelled."
    rm -f "$LOG_FILE"
fi
