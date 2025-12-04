from flask import Flask, render_template


app = Flask(__name__)

@app.route("/")
def index():
    return render_template("index.html")

@app.route("/tab_one")
def tab_one():
    return render_template("tab_one.html")

@app.route("/tab_two")
def tab_two():
    return render_template("tab_two.html")

@app.route("/tab_three")
def tab_three():
    return render_template("tab_three.html")

@app.route("/configure")
def tab_configure():
    return render_template("configure.html")

if __name__ == "__main__":
    app.run(debug=True, port=3978)