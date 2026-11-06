#!/bin/bash
set -e

cd /home/soilmonitor/IoT_project_integrated

# Automatically load the API URL and key
set -a
source ./.env
set +a

# Activate the existing Python environment
source /home/soilmonitor/soil-monitor/.venv/bin/activate

# Run the existing 30-minute sensor program
python main.py
