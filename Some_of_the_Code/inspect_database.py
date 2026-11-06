import sqlite3
import sys

db_path = sys.argv[1]

connection = sqlite3.connect(
    "file:" + db_path.replace("\\", "/") + "?mode=ro",
    uri=True
)

print("TABLES:")
tables = connection.execute("""
    SELECT name, sql
    FROM sqlite_master
    WHERE type = 'table'
      AND name NOT LIKE 'sqlite_%'
""").fetchall()

for table in tables:
    print(table)

print("\nREADING COUNT:")
print(connection.execute(
    "SELECT COUNT(*) FROM moisture_readings"
).fetchone()[0])

print("\nLATEST 5 READINGS:")
rows = connection.execute("""
    SELECT *
    FROM moisture_readings
    ORDER BY id DESC
    LIMIT 5
""").fetchall()

for row in rows:
    print(row)

connection.close()
