from flask import Flask, render_template
from faker import Faker

app = Flask(__name__)
fake = Faker()

def setup_routes(app):
    @app.route('/PersonalTab')
    def personal_tab():
        return render_template('personalTab.html')