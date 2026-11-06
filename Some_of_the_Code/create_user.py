
import getpass
import os
import mysql.connector
from werkzeug.security import generate_password_hash

username = input("Choose a username: ").strip()
email = input("Email (optional): ").strip() or None
password = getpass.getpass("Choose a password: ")
confirm = getpass.getpass("Confirm password: ")

if not username:
    raise SystemExit("Username cannot be empty.")

if len(password) < 12:
    raise SystemExit("Use a password with at least 12 characters.")

if password != confirm:
    raise SystemExit("Passwords do not match.")

db = mysql.connector.connect(
    host=os.environ.get("IOT_MYSQL_HOST", "127.0.0.1"),
    port=int(os.environ.get("IOT_MYSQL_PORT", "3306")),
    user=os.environ.get("IOT_MYSQL_USER", "root"),
    password=os.environ.get("IOT_MYSQL_PASSWORD"),
    database=os.environ.get("IOT_MYSQL_DATABASE", "comp717_iot"),
)

try:
    cursor = db.cursor()
    cursor.execute(
        "SELECT UserID FROM Users WHERE Username = %s",
        (username,)
    )
    if cursor.fetchone():
        raise SystemExit("That username already exists.")

    password_hash = generate_password_hash(password)
    cursor.execute(
        """
        INSERT INTO Users (Username, Password, Email, Role)
        VALUES (%s, %s, %s, %s)
        """,
        (username, password_hash, email, "user")
    )
    db.commit()
    print("Account created successfully.")
finally:
    db.close()
