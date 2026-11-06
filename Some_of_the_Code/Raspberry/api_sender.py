"""Part C: sends a reading from Rilo's monitoring program to the API."""
import json
import os
from urllib.request import Request, urlopen
from urllib.error import URLError, HTTPError

API_URL = os.environ.get('IOT_API_URL','http://127.0.0.1:5000/api/readings')

def send_reading(timestamp, raw_value, moisture, status, source, irrigation):
    payload = {'timestamp':timestamp, 'raw_value':raw_value,
               'moisture_percent':round(moisture,2), 'status':status,
               'source':source, 'irrigation_required':irrigation}
    headers={'Content-Type':'application/json'}
    if os.environ.get('IOT_API_KEY'):
        headers['X-API-Key']=os.environ['IOT_API_KEY']
    req=Request(API_URL,data=json.dumps(payload).encode('utf-8'),headers=headers,method='POST')
    try:
        with urlopen(req,timeout=5) as response:
            return response.status == 201
    except (URLError, HTTPError, TimeoutError, OSError) as exc:
        print(f'[API] Send failed (monitoring continues): {exc}')
        return False
