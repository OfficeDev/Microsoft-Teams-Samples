const express = require('express');
const bodyparser = require('body-parser');
const path = require('path');
const app = express();
const fs = require("fs");
let productDetailsList = {
  "productDetails": [{
    "productId": "01SD001",
    "productName": "Laptop",
    "image": null,
    "status": null,
  },
  {
    "productId": "01DU890",
    "productName": "Desktop",
    "image": null,
    "status": null,
  },
  {
    "productId": "01PM998",
    "productName": "Mobile",
    "image": null,
    "status": null,
  },
  {
    "productId": "01NT789",
    "productName": "Tablet",
    "image": null,
    "status": null,
  },
  {
    "productId": "01EW420",
    "productName": "IOS device",
    "image": null,
    "status": null,
  }]
};

app.use(express.static(__dirname + '/Styles'));
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);
app.use(express.json());
app.use("/Images", express.static(path.resolve(__dirname, 'Images')));

app.get('/tab', function (req, res) {
  res.render('./views/tab');
});

app.get('/productDetails', function (req, res) {
  var productId = req.url.split('=')[1];
  var productDetails = {
    "productId": null,
    "productName": null,
    "image": null,
    "status": null
  }
  
  var currentData = productDetailsList["productDetails"];

  currentData.find((product) => {
    if (product.productId == productId) {
      productDetails = product;
    }
  });

  res.render('./views/productDetail', { productDetails: JSON.stringify(productDetails) });
});

app.get('/scanProduct', function (req, res) {
  res.render('./views/scanProduct');
});

app.get('/productList', function (req, res) {
  res.render('./views/productList');
});

app.get('/viewProductDetail', function (req, res) {
  res.render('./views/viewProductDetail');
});

app.post('/Save', (req, res, next) => {
  var productDetail = {
    "productId": req.body.productId,
    "productName": req.body.productName,
    "image": req.body.image,
    "status": req.body.status
  };
  var currentData = productDetailsList["productDetails"];
  let updateindex;
  currentData.map((product, index) => {
    if (product.productId == productDetail.productId) {
      updateindex = index;
    }
  })
  currentData[updateindex] = productDetail;
  productDetailsList["productDetails"] = currentData;
});

app.listen(3978, function () {
  console.log('app listening on port 3978!');
});