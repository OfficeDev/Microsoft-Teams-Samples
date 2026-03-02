from flask import Flask, render_template
import os

app = Flask(__name__)

PORT = int(os.getenv("PORT", 3978))

@app.route("/")
def home():
    return render_template("tab.html")

@app.route("/tab")
def tab():
    return render_template("tab.html")


if __name__ == "__main__":
    app.run(debug=True, port=PORT)
    