from flask import Flask, request, render_template, send_from_directory, jsonify
import json
import os

app = Flask(__name__)
app.config['UPLOAD_FOLDER'] = 'Images'

product_details_list = {
    "productDetails": [
        {"productId": "01SD001", "productName": "Laptop", "image": None, "status": None},
        {"productId": "01DU890", "productName": "Desktop", "image": None, "status": None},
        {"productId": "01PM998", "productName": "Mobile", "image": None, "status": None}
    ]
}

@app.route('/tab')
def tab():
    return render_template('tab.html')

@app.route('/productDetails')
def product_details():
    product_id = request.args.get('productId')
    product_detail = next((product for product in product_details_list["productDetails"] if product["productId"] == product_id),
                          {"productId": None, "productName": None, "image": None, "status": None})
    return render_template('productDetail.html', productDetails=json.dumps(product_detail))

@app.route('/scanProduct')
def scan_product():
    return render_template('scanProduct.html')

@app.route('/viewProductDetail')
def view_product_detail():
    return render_template('viewProductDetail.html')

@app.route('/save', methods=['POST'])
def save():
    data = json.loads(request.form['data'])
    product_detail = {
        "productId": data["productId"],
        "productName": data["productName"],
        "image": data.get("image"),
        "status": data.get("status")
    }

    current_data = product_details_list["productDetails"]
    for index, product in enumerate(current_data):
        if product["productId"] == product_detail["productId"]:
            current_data[index] = product_detail

    product_details_list["productDetails"] = current_data
    return '', 200

@app.route('/Images/<path:filename>')
def get_image(filename):
    return send_from_directory(app.config['UPLOAD_FOLDER'], filename)

if __name__ == "__main__":
    port = int(os.environ.get('PORT', 3978))
    app.run(host='0.0.0.0', port=port, debug=True)
