from flask import render_template


def setup_routes(app):
    @app.route('/PersonalTab')
    def personal_tab():
        return render_template('personalTab.html')