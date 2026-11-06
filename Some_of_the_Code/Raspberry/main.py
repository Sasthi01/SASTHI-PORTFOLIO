
"""Run the team's moisture monitor for a bounded 30-minute test session."""
import random
import time
from datetime import datetime
from api_sender import send_reading

from moisture_monitor import (
    SIMULATION_MODE, DRY_VALUE, WET_VALUE, READING_INTERVAL, SESSION_DURATION,
    initialise_database, check_sensor_connection, read_sensor,
    calculate_moisture, get_status, determine_irrigation,
    save_reading_db, save_reading_csv,
)


def main():
    initialise_database()
    print('Soil Moisture Monitoring System')
    print('MODE:', 'SIMULATION' if SIMULATION_MODE else 'REAL SENSOR')
    print(f'Session limit: {SESSION_DURATION // 60} minutes')
    print('The sensor remains electrically powered while the Pi is powered.')
    print('Press Ctrl+C to stop early.\n')

    start = time.monotonic()
    readings = 0
    try:
        while time.monotonic() - start < SESSION_DURATION:
            try:
                if SIMULATION_MODE:
                    raw_value = random.randint(min(DRY_VALUE, WET_VALUE), max(DRY_VALUE, WET_VALUE))
                    source = 'SIMULATED'
                else:
                    from smbus2 import SMBus
                    with SMBus(1) as bus:
                        if not check_sensor_connection(bus):
                            raise OSError('ADC at 0x48 did not respond')
                        raw_value = read_sensor(bus)
                    source = 'REAL'

                moisture = calculate_moisture(raw_value)
                status = get_status(moisture)
                timestamp = datetime.now().astimezone().isoformat(timespec='seconds')
                reading_id = save_reading_db(timestamp, raw_value, moisture, status, source)
                try:
                    save_reading_csv(reading_id, timestamp, raw_value, moisture, status, source)
                    saved = 'SQLite + CSV'
                except (OSError, ValueError) as exc:
                    saved = 'SQLite only (CSV error)'
                    print(f'[WARNING] CSV save failed: {exc}')
                # Send the reading to the Flask API.
                # SQLite and CSV remain the local backups.
                try:
                    delivered = send_reading(
                        timestamp,
                        raw_value,
                        moisture,
                        status,
                        source,
                        determine_irrigation(status)
                    )

                    if delivered:
                        print('[API] Reading delivered successfully.')
                    else:
                        print('[API] Delivery failed; local reading retained.')

                except Exception as exc:
                    print(f'[API] Unexpected error: {exc}')
                readings += 1
                print(f'[{timestamp}] Raw: {raw_value} | Moisture: {moisture:.1f}% | '
                      f'Status: {status} | Source: {source} | Saved: {saved}')
                print(f'Irrigation advisory: {determine_irrigation(status)}')
            except (OSError, ImportError) as exc:
                print(f'[ERROR] Sensor communication failed; reading not saved: {exc}')
            remaining = SESSION_DURATION - (time.monotonic() - start)
            if remaining > 0:
                wait = min(READING_INTERVAL, remaining)
                print(f'[WAITING] Next reading in {wait:.0f} seconds (sensor power remains ON).\n')
                time.sleep(wait)
    except KeyboardInterrupt:
        print('\nMonitoring stopped by user.')
    finally:
        print(f'Session ended. Readings collected: {readings}.')
        print('IMPORTANT: Shut down the Raspberry Pi and disconnect its power to unpower the sensor.')


if __name__ == '__main__':
    main()
