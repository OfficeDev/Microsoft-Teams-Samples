const express = require('express');
const bodyparser = require('body-parser');
const path = require('path');
const app = express();
const fs = require("fs");
let multer = require('multer');
let upload = multer({ storage: multer.memoryStorage(),
  limits: { fieldSize: 25 * 1024 * 1024 }});
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

app.get('/viewProductDetail', function (req, res) {
  res.render('./views/viewProductDetail');
});

app.post('/save', upload.single('data'), function(req, res){
  var data = req.body.data
  var result = JSON.parse(data);
  var productDetail = {
    "productId": result.productId,
    "productName": result.productName,
    "image": result.image,
    "status": result.status
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

  res.send();
});

app.listen(3978, function () {
  console.log('app listening on port 3978!');
});