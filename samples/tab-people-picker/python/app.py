# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
from flask import Flask, render_template
import os

app = Flask(__name__, template_folder='templates')

@app.route('/configure')
def configure():
    return render_template('configure.html')

@app.route('/tab')
def tab():
    return render_template('tab.html')

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 3978))
    print(f"Bot/ME service listening at http://localhost:{port}")
    app.run(host='0.0.0.0', port=port)
