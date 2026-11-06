
import os
import getpass
import mysql.connector
from werkzeug.security import generate_password_hash

username = input("Enter your existing username: ").strip()
password = getpass.getpass("Enter your new password: ")
confirm = getpass.getpass("Confirm your new password: ")

if not username or not password:
    raise SystemExit("Username and password cannot be empty.")

if password != confirm:
    raise SystemExit("Passwords do not match.")

if len(password) < 12:
    raise SystemExit("Use at least 12 characters.")

connection = mysql.connector.connect(
    host=os.environ.get("IOT_MYSQL_HOST", "127.0.0.1"),
    port=int(os.environ.get("IOT_MYSQL_PORT", "3306")),
    user=os.environ.get("IOT_MYSQL_USER", "root"),
    password=os.environ.get("IOT_MYSQL_PASSWORD", ""),
    database=os.environ.get("IOT_MYSQL_DATABASE", "comp717_iot"),
    connection_timeout=5,
)

try:
    cursor = connection.cursor()
    cursor.execute(
        "UPDATE Users SET Password = %s WHERE Username = %s",
        (generate_password_hash(password), username),
    )
    connection.commit()

    if cursor.rowcount == 1:
        print("Password updated successfully!")
    else:
        print("No account updated. Check your username.")

    cursor.close()
finally:
    connection.close()
