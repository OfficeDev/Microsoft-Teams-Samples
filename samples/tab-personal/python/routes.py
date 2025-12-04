from flask import Flask, render_template


app = Flask(__name__)
def setup_routes(app):
    @app.route('/PersonalTab')
    def personal_tab():
        return render_template('personalTab.html')