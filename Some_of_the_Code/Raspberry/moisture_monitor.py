"""Sensor readings, calibration and persistence for the IoT soil monitor."""
import csv
import sqlite3
import time
from pathlib import Path

SIMULATION_MODE = False
READING_INTERVAL = 300  # seconds between readings
SESSION_DURATION = 30 * 60  # 30-minute monitoring session
DRY_VALUE = 1210
WET_VALUE = 13990
ADC_ADDRESS = 0x48
CONFIG_REGISTER = 0x01
CONVERSION_REGISTER = 0x00
ADC_CONFIG = 0xC383  # ADS1115 A0 single-ended, +/-4.096 V, single-shot 128 SPS
BASE_DIR = Path(__file__).resolve().parent
DB_PATH = BASE_DIR / 'Data' / 'soil_moisture.db'
CSV_PATH = BASE_DIR / 'Data' / 'soil_readings.csv'


def initialise_database():
    DB_PATH.parent.mkdir(parents=True, exist_ok=True)
    with sqlite3.connect(DB_PATH) as conn:
        conn.execute('''CREATE TABLE IF NOT EXISTS moisture_readings (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            timestamp TEXT NOT NULL,
            raw_value INTEGER NOT NULL,
            moisture_percent REAL NOT NULL,
            status TEXT NOT NULL,
            source TEXT NOT NULL,
            irrigation_required TEXT NOT NULL DEFAULT 'IRRIGATION_NOT_REQUIRED'
        )''')
        columns = {row[1] for row in conn.execute('PRAGMA table_info(moisture_readings)')}
        if 'irrigation_required' not in columns:
            conn.execute("ALTER TABLE moisture_readings ADD COLUMN irrigation_required TEXT NOT NULL DEFAULT 'IRRIGATION_NOT_REQUIRED'")
            conn.execute("UPDATE moisture_readings SET irrigation_required = CASE WHEN status = 'DRY' THEN 'IRRIGATION_REQUIRED' ELSE 'IRRIGATION_NOT_REQUIRED' END")


        # Import existing team CSV once when starting with an empty database.
        if CSV_PATH.exists() and conn.execute('SELECT COUNT(*) FROM moisture_readings').fetchone()[0] == 0:
            with CSV_PATH.open(newline='', encoding='utf-8-sig') as file:
                for row in csv.DictReader(file):
                    if row.get('source') != 'REAL':
                        continue
                    status = row['status']
                    conn.execute("""INSERT OR IGNORE INTO moisture_readings
                        (id, timestamp, raw_value, moisture_percent, status, source, irrigation_required)
                        VALUES (?, ?, ?, ?, ?, ?, ?)""",
                        (int(row['id']), row['timestamp'], int(row['raw_value']),
                         float(row['moisture_percent']), status, row['source'],
                         row.get('irrigation_required') or determine_irrigation(status)))


def check_sensor_connection(bus):
    """Check ADC communication, not physical probe presence."""
    try:
        return len(bus.read_i2c_block_data(ADC_ADDRESS, CONFIG_REGISTER, 2)) == 2
    except OSError:
        return False


def read_sensor(bus):
    bus.write_i2c_block_data(ADC_ADDRESS, CONFIG_REGISTER,
                             [(ADC_CONFIG >> 8) & 0xFF, ADC_CONFIG & 0xFF])
    time.sleep(0.02)
    data = bus.read_i2c_block_data(ADC_ADDRESS, CONVERSION_REGISTER, 2)
    raw = (data[0] << 8) | data[1]
    return raw - 65536 if raw >= 32768 else raw


def calculate_moisture(raw_value, dry_value=DRY_VALUE, wet_value=WET_VALUE):
    if dry_value == wet_value:
        raise ValueError('Dry and wet calibration values must differ')
    return max(0.0, min(100.0, (raw_value - dry_value) * 100.0 / (wet_value - dry_value)))


def get_status(moisture):
    return 'DRY' if moisture < 30 else 'MODERATE' if moisture < 70 else 'WET'


def determine_irrigation(status):
    return 'IRRIGATION_REQUIRED' if status == 'DRY' else 'IRRIGATION_NOT_REQUIRED'


def save_reading_db(timestamp, raw_value, moisture, status, source):
    irrigation = determine_irrigation(status)
    with sqlite3.connect(DB_PATH) as conn:
        cursor = conn.execute('''INSERT INTO moisture_readings
            (timestamp, raw_value, moisture_percent, status, source, irrigation_required)
            VALUES (?, ?, ?, ?, ?, ?)''',
            (timestamp, raw_value, moisture, status, source, irrigation))
        return cursor.lastrowid


def save_reading_csv(reading_id, timestamp, raw_value, moisture, status, source):
    CSV_PATH.parent.mkdir(parents=True, exist_ok=True)
    exists = CSV_PATH.exists() and CSV_PATH.stat().st_size > 0
    with CSV_PATH.open('a', newline='', encoding='utf-8') as file:
        writer = csv.writer(file)
        if not exists:
            writer.writerow(['id', 'timestamp', 'raw_value', 'moisture_percent',
                             'status', 'source', 'irrigation_required'])
        writer.writerow([reading_id, timestamp, raw_value, round(moisture, 2),
                         status, source, determine_irrigation(status)])
