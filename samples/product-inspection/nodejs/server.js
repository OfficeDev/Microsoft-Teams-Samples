const express = require('express');
const bodyparser = require('body-parser');
const app = express();
const fs = require("fs");

app.use(express.static(__dirname + '/Styles'));
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);
app.use(express.json());

app.get('/tab', function (req, res) {
  res.render('./views/tab');
});

app.get('/productDetails', function (req, res) {
  var productId = req.url.split('=')[1];
  var productDetails = {
    "productId": null,
    "image": null,
    "status": null
  }
  const fileJsonString = fs.readFileSync("../nodejs/public/productDetail.json", "utf8");

  if (fileJsonString == "") {
    res.render('./views/productDetail', { productDetails: JSON.stringify(productDetails) });
  }
  else {
    let productDetailsData = JSON.parse(fileJsonString);
    productDetailsData.find((product) => {
      
      if (product.productId == productId) {
        productDetails = product;
      }
    })
  }
  res.render('./views/productDetail', { productDetails: JSON.stringify(productDetails) });
});

app.get('/scanProduct', function (req, res) {
  res.render('./views/scanProduct');
});

app.get('/viewProductDetail', function (req, res) {
  res.render('./views/viewProductDetail');
});

app.post('/Save', (req, res, next) => {
  var productDetail = {
    "productId": req.body.productId,
    "image": req.body.image,
    "status": req.body.status
  };

  const fileJsonString = fs.readFileSync("../nodejs/public/productDetail.json", "utf8");

  if (fileJsonString == "") {
    fs.writeFileSync("../nodejs/public/productDetail.json", JSON.stringify([productDetail]), "utf-8");
  }
  else {
    let productDetailsData = JSON.parse(fileJsonString);
    productDetailsData.push(productDetail);
    fs.writeFileSync("../nodejs/public/productDetail.json", JSON.stringify(productDetailsData), "utf-8");
  }
});

app.listen(3978, function () {
  console.log('app listening on port 3978!');
});