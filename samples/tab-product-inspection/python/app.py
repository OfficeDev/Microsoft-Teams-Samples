# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import os
import json
from flask import Flask, jsonify, request, render_template, send_from_directory

app = Flask(__name__, static_folder='static')
app.config['UPLOAD_FOLDER'] = os.path.join(os.getcwd(), 'Images')
app.config['MAX_CONTENT_LENGTH'] = 25 * 1024 * 1024  # 25 MB limit

# Data simulating a database
productDetailsList = {
    "productDetails": [
        {
            "productId": "01SD001",
            "productName": "Laptop",
            "image": None,
            "status": None,
        },
        {
            "productId": "01DU890",
            "productName": "Desktop",
            "image": None,
            "status": None,
        },
        {
            "productId": "01PM998",
            "productName": "Mobile",
            "image": None,
            "status": None,
        }
    ]
}

# Routes

@app.route('/tab')
def tab():
    return render_template('tab.html')


@app.route('/scanProduct')
def scan_product():
    return render_template('scanProduct.html')


@app.route('/viewProductDetail')
def view_product_detail():
    return render_template('viewProductDetail.html')


@app.route('/productDetails')
def product_details():
    product_id = request.args.get("productId")
    product_details = {
        "productId": None,
        "productName": None,
        "image": None,
        "status": None
    }

    for product in productDetailsList["productDetails"]:
        if product["productId"] == product_id:
            product_details = product
            break

    return render_template('productDetail.html', productDetails=json.dumps(product_details))


@app.route('/save', methods=['POST'])
def save_product():
    try:
        product_id = request.form.get('productId')
        status = request.form.get('status')
        image_file = request.files.get('image')

        if not product_id or not status or not image_file:
            return jsonify({"error": "Missing data"}), 400

        filename = f"{product_id}_captured_image.png"
        filepath = os.path.join(app.config['UPLOAD_FOLDER'], filename)
        image_file.save(filepath)

        for product in productDetailsList["productDetails"]:
            if product["productId"] == product_id:
                product["image"] = f"/Images/{filename}"
                product["status"] = status
                break

        print(f"[SAVE] {product_id}: {status} saved → {filename}")
        return jsonify({"message": "Saved"}), 200

    except Exception as e:
        print("Save failed:", str(e))
        return jsonify({"error": "Server error"}), 500


@app.route('/Images/<filename>')
def serve_image(filename):
    return send_from_directory(app.config['UPLOAD_FOLDER'], filename)

@app.route('/api/productDetails')
def get_product_details():
    product_id = request.args.get('productId')
    for product in productDetailsList["productDetails"]:
        if product["productId"] == product_id:
            return jsonify(product)
    return jsonify({"error": "Not found"}), 404

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 3978))
    app.run(host='0.0.0.0', port=port, debug=True)
