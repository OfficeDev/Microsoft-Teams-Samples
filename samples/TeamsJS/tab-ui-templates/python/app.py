from flask import Flask
from routes import setup_routes

def create_app():
    app = Flask(__name__)

    # Serve static files from 'static' directory
    app.static_folder = 'static'
    app.template_folder = 'templates'

    # Setup application routes
    setup_routes(app)

    return app

if __name__ == '__main__':
    app = create_app()
    app.run(debug=True, port=3978)