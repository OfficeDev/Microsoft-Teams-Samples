import os
from flask import Flask, render_template, request
from helpers import auth
from config import DefaultConfig

app = Flask(__name__, static_folder='Styles', template_folder='views')
app.config.from_object(DefaultConfig)

@app.route('/configure')
def configure():
    return render_template('configure.html')

@app.route('/lifeCycleDemo')
def life_cycle_demo():
    tenant_id = request.args.get('tid')
    if not tenant_id:
        return "Tenant ID not provided", 400

    token = auth.get_access_token(tenant_id)
    return render_template('lifeCycleDemo.html', token=token)

if __name__ == '__main__':
    app.run(host="0.0.0.0", port=3978, debug=True)
