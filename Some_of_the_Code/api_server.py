"""Part C: HTTP API for IoT soil-moisture readings."""
import os
import sqlite3
from pathlib import Path
from contextlib import closing
from datetime import datetime
from flask import (
    Flask, jsonify, request, send_from_directory,
    redirect, url_for, render_template_string,
    session, flash
)
from flask_login import (
    LoginManager, UserMixin, login_user,
    login_required, logout_user, current_user
)
from werkzeug.security import generate_password_hash, check_password_hash

DB_ENGINE = os.environ.get("IOT_DB_ENGINE", "mysql").lower()
SENSOR_ID = int(os.environ.get("IOT_SENSOR_ID", "1"))

def mysql_connect():
    import mysql.connector
    return mysql.connector.connect(
        host=os.environ.get("IOT_MYSQL_HOST", "127.0.0.1"),
        port=int(os.environ.get("IOT_MYSQL_PORT", "3306")),
        user=os.environ.get("IOT_MYSQL_USER", "root"),
        password=os.environ.get("IOT_MYSQL_PASSWORD", "Vdayal*333"),
        database=os.environ.get("IOT_MYSQL_DATABASE", "comp717_iot"),
        connection_timeout=5,
    )


def mysql_rows(sql, params=()):
    db=mysql_connect()
    try:
        cur=db.cursor(dictionary=True)
        try:
            cur.execute(sql, params)
            return cur.fetchall()
        finally: cur.close()
    finally: db.close()

def mysql_insert(stamp, raw, moisture, status, irrigation):
    db=mysql_connect()
    try:
        cur=db.cursor()
        try:
            cur.execute("""INSERT INTO Sensor_Data
                (SensorID, `Timestamp`, Raw_value, Moisture_percent, Status, Irrigation_required)
                VALUES (%s,%s,%s,%s,%s,%s)""",
                (SENSOR_ID, datetime.fromisoformat(stamp.replace("Z", "+00:00")).replace(tzinfo=None), raw, moisture, status, int(irrigation=="IRRIGATION_REQUIRED")))
            db.commit()
            return cur.lastrowid
        finally: cur.close()
    except:
        db.rollback()
        raise
    finally: db.close()

def format_mysql(row):
    return {"id":row["Sensor_dataID"],"sensor_id":row["SensorID"],
            "timestamp":row["Timestamp"].isoformat() if hasattr(row["Timestamp"],"isoformat") else str(row["Timestamp"]),
            "raw_value":row["Raw_value"],"moisture_percent":float(row["Moisture_percent"]),
            "status":row["Status"],"irrigation_required":"IRRIGATION_REQUIRED" if row["Irrigation_required"] else "IRRIGATION_NOT_REQUIRED",
            "source":"UNKNOWN"}


app = Flask(__name__)

# Set a unique secret through an environment variable.
app.config["SECRET_KEY"] = os.environ.get("IOT_FLASK_SECRET_KEY")

if not app.config["SECRET_KEY"]:
    raise RuntimeError(
        "Set the IOT_FLASK_SECRET_KEY environment variable before starting the app."
    )

login_manager = LoginManager()
login_manager.init_app(app)
login_manager.login_view = "login"

class User(UserMixin):
    def __init__(self, user_id, username, password_hash):
        self.id = str(user_id)
        self.username = username
        self.password_hash = password_hash


@login_manager.user_loader
def load_user(user_id):
    rows = mysql_rows(
        """
        SELECT UserID, Username, Password
        FROM Users
        WHERE UserID = %s
        LIMIT 1
        """,
        (user_id,)
    )

    if not rows:
        return None

    row = rows[0]
    return User(
        row["UserID"],
        row["Username"],
        row["Password"]
    )


LOGIN_PAGE = """
<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Garden Soil Monitor - Login</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background: #f1f5ed;
            display: grid;
            place-items: center;
            min-height: 100vh;
            margin: 0;
        }
        .login-card {
            background: white;
            padding: 2rem;
            border-radius: 14px;
            width: min(340px, calc(100% - 64px));
            box-shadow: 0 6px 24px #00000012;
        }
        h1 { color: #315b35; font-size: 1.5rem; }
        label { display: block; margin-top: 1rem; }
        input {
            box-sizing: border-box;
            width: 100%;
            padding: 0.75rem;
            margin-top: 0.35rem;
        }
        button {
            width: 100%;
            margin-top: 1.25rem;
            padding: 0.8rem;
            background: #315b35;
            color: white;
            border: 0;
            border-radius: 6px;
            cursor: pointer;
        }
        .message { color: #a12626; }
    </style>
</head>
<body>
    <main class="login-card">
        <h1>Garden Soil Monitor</h1>
        <p>Sign in to view sensor readings.</p>
        {% for message in get_flashed_messages() %}
            <p class="message">{{ message }}</p>
        {% endfor %}
        <form method="post">
            <label for="username">Username</label>
            <input id="username" name="username"
                   autocomplete="username" required>
            <label for="password">Password</label>
            <input id="password" name="password" type="password"
                   autocomplete="current-password" required>
            <button type="submit">Log in</button>
        </form>
    </main>
</body>
</html>
"""


@app.route("/login", methods=["GET", "POST"])
def login():
    if current_user.is_authenticated:
        return redirect(url_for("dashboard"))

    if request.method == "POST":
        username = request.form.get("username", "").strip()
        password = request.form.get("password", "")

        rows = mysql_rows(
            """
            SELECT UserID, Username, Password
            FROM Users
            WHERE Username = %s
            LIMIT 1
            """,
            (username,)
        )

        if rows and check_password_hash(rows[0]["Password"], password):
            row = rows[0]
            user = User(row["UserID"], row["Username"], row["Password"])
            login_user(user)
            return redirect(url_for("dashboard"))

        flash("Invalid username or password.")

    return render_template_string(LOGIN_PAGE)


@app.post("/logout")
@login_required
def logout():
    logout_user()
    return redirect(url_for("login"))

@app.get("/")
@login_required
def dashboard():
    """Serve the Garden Soil Monitor dashboard."""
    return send_from_directory(
        Path(__file__).parent,
        "dashboard.html"
    )

DB_PATH = Path(os.environ.get("IOT_API_DB", str(Path(__file__).parent / "api_readings.db")))
API_KEY = os.environ.get("IOT_API_KEY", "")
STATUSES = {"DRY", "MODERATE", "WET"}
SOURCES = {"REAL", "SIMULATED"}

def connect():
    DB_PATH.parent.mkdir(parents=True, exist_ok=True)
    conn = sqlite3.connect(DB_PATH, timeout=10)
    conn.row_factory = sqlite3.Row
    return conn

def init_db():
    with closing(connect()) as db, db:
        db.execute("""CREATE TABLE IF NOT EXISTS readings (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            timestamp TEXT NOT NULL,
            raw_value INTEGER NOT NULL,
            moisture_percent REAL NOT NULL,
            status TEXT NOT NULL,
            source TEXT NOT NULL,
            irrigation_required TEXT NOT NULL
        )""")

@app.get('/api/health')
def health():
    return jsonify(status='ok', service='COMP717 Part C API')

@app.post('/api/readings')
def receive():
    if API_KEY and request.headers.get('X-API-Key') != API_KEY:
        return jsonify(error='Unauthorized'), 401
    data = request.get_json(silent=True)
    if not isinstance(data, dict):
        return jsonify(error='Expected a JSON object'), 400
    try:
        raw = data['raw_value']
        moisture = data['moisture_percent']
        status = data['status']
        source = data['source']
        irrigation = data['irrigation_required']
        stamp = data.get('timestamp') or datetime.now().astimezone().isoformat(timespec='seconds')
        if type(raw) is not int or type(moisture) not in (int,float) or not 0 <= moisture <= 100:
            raise ValueError('Invalid sensor values')
        if status not in STATUSES or source not in SOURCES or irrigation not in ('IRRIGATION_REQUIRED','IRRIGATION_NOT_REQUIRED'):
            raise ValueError('Invalid classification')
        datetime.fromisoformat(stamp.replace('Z','+00:00'))
    except (KeyError, TypeError, ValueError) as exc:
        return jsonify(error=f'Invalid reading: {exc}'), 400
    if DB_ENGINE == "mysql":
        try: new_id=mysql_insert(stamp,raw,moisture,status,irrigation)
        except Exception as exc:
            app.logger.exception("MySQL insert failed")
            return jsonify(error="Database unavailable or sensor ID missing"),503
        return jsonify(message="Reading received",id=new_id),201
    with closing(connect()) as db, db:
        cursor=db.execute('''INSERT INTO readings (timestamp,raw_value,moisture_percent,status,source,irrigation_required)
                          VALUES (?,?,?,?,?,?)''', (stamp,raw,moisture,status,source,irrigation))
        new_id=cursor.lastrowid
    return jsonify(message='Reading received', id=new_id), 201

@app.get('/api/readings/latest')
@login_required
def latest():
    if DB_ENGINE == "mysql":
        try:
            rows=mysql_rows("SELECT * FROM Sensor_Data ORDER BY Sensor_dataID DESC LIMIT 1")
            if not rows: return jsonify(error="No readings available"),404
            return jsonify(format_mysql(rows[0]))
        except Exception:
            app.logger.exception("MySQL read failed")
            return jsonify(error="Database unavailable"),503
    with closing(connect()) as db, db:
        row=db.execute('SELECT * FROM readings ORDER BY id DESC LIMIT 1').fetchone()
    if row is None:
        return jsonify(error='No readings available'), 404
    return jsonify(dict(row))

@app.get('/api/readings/history')
@login_required
def history():
    try:
        limit=int(request.args.get('limit','50'))
        if not 1 <= limit <= 500: raise ValueError()
    except ValueError:
        return jsonify(error='limit must be between 1 and 500'),400
    if DB_ENGINE == "mysql":
        try:
            rows=mysql_rows("SELECT * FROM Sensor_Data ORDER BY Sensor_dataID DESC LIMIT %s",(limit,))
            return jsonify(readings=[format_mysql(row) for row in rows],count=len(rows))
        except Exception:
            app.logger.exception("MySQL history failed")
            return jsonify(error="Database unavailable"),503
    with closing(connect()) as db, db:
        rows=db.execute('SELECT * FROM readings ORDER BY id DESC LIMIT ?', (limit,)).fetchall()
    return jsonify(readings=[dict(row) for row in rows], count=len(rows))

if DB_ENGINE == "sqlite":
    init_db()
elif DB_ENGINE != "mysql":
    raise ValueError("IOT_DB_ENGINE must be sqlite or mysql")
if __name__=='__main__':
    app.run(host=os.environ.get('IOT_API_HOST','127.0.0.1'),port=int(os.environ.get('IOT_API_PORT','5000')),debug=False)
