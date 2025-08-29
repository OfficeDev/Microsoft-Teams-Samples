import os
import json
from flask import Flask, request, jsonify, render_template
import requests
from urllib.parse import urlencode
from dotenv import load_dotenv
import auth

# Load environment variables
ENV_FILE = os.path.join(os.path.dirname(__file__), '.env')
load_dotenv(ENV_FILE)

# Global storage for request data (in production, use a database)
local_data = []

app = Flask(__name__)

# Configure Flask to use static files and templates
app.static_folder = 'static'
app.template_folder = 'views'

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/UserNotification')
def user_notification():
    tenant_id = os.getenv('TenantId')
    
    try:
        token = auth.get_access_token(tenant_id)
        request_data = {
            "requestDetails": local_data,
            "token": token
        }
        return render_template('UserNotification.html', data=json.dumps(request_data))
    except Exception as e:
        print(f"Error getting access token: {e}")
        request_data = {
            "requestDetails": local_data,
            "token": None
        }
        return render_template('UserNotification.html', data=json.dumps(request_data))

@app.route('/auth/auth-start')
def auth_start():
    client_id = os.getenv('ClientId')
    return render_template('auth-start.html', clientId=json.dumps(client_id))

@app.route('/auth/auth-end')
def auth_end():
    client_id = os.getenv('ClientId')
    return render_template('auth-end.html', clientId=json.dumps(client_id))

@app.route('/UserRequest')
def user_request():
    request_id = None
    if '=' in request.url:
        request_id = request.url.split('=')[1]
    
    request_data = {}
    if request_id:
        for item in local_data:
            if str(item.get('id')) == str(request_id):
                request_data = item
                break
    
    return render_template('UserRequest.html', data=json.dumps(request_data))

@app.route('/ApproveRejectRequestActivity', methods=['POST'])
def approve_reject_request_activity():
    print('Activity server calling')
    data = request.get_json()
    
    for item in local_data:
        if str(item.get('id')) == str(data.get('taskId')):
            item['status'] = data.get('status')
            break
    
    return render_template('UserRequest.html', data=json.dumps('successfully'))

@app.route('/ApproveRejectRequest', methods=['POST'])
def approve_reject_request():
    print('Server calling')
    data = request.get_json()
    
    for item in local_data:
        if str(item.get('id')) == str(data.get('taskId')):
            item['status'] = data.get('status')
            break
    
    return '', 200

@app.route('/SaveRequest', methods=['POST'])
def save_request():
    data = request.get_json()
    
    task_details = {
        'id': data.get('id'),
        'title': data.get('title'),
        'description': data.get('description'),
        'assignedTo': data.get('assignedTo'),
        'createdBy': data.get('createdBy'),
        'status': 'Pending'
    }
    
    local_data.append(task_details)
    return '', 200

@app.route('/auth/token', methods=['POST'])
def auth_token():
    data = request.get_json()
    tid = data.get('tid')
    token = data.get('token')
    scopes = ["https://graph.microsoft.com/User.Read"]
    
    url = f"https://login.microsoftonline.com/{tid}/oauth2/v2.0/token"
    
    params = {
        'client_id': os.getenv('ClientId'),
        'client_secret': os.getenv('ClientSecret'),
        'grant_type': 'urn:ietf:params:oauth:grant-type:jwt-bearer',
        'assertion': token,
        'requested_token_use': 'on_behalf_of',
        'scope': ' '.join(scopes)
    }
    
    headers = {
        'Accept': 'application/json',
        'Content-Type': 'application/x-www-form-urlencoded'
    }
    
    try:
        response = requests.post(url, data=urlencode(params), headers=headers)
        
        if response.status_code != 200:
            error_data = response.json()
            return jsonify({"error": error_data.get("error")}), response.status_code
        else:
            result = response.json()
            return jsonify(result.get('access_token'))
    except Exception as e:
        print(f"Error in token exchange: {e}")
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    port = int(os.getenv('PORT', 3978))
    print(f'Tab Request Approval Python service listening at http://localhost:{port}')
    app.run(host='0.0.0.0', port=port, debug=True)
