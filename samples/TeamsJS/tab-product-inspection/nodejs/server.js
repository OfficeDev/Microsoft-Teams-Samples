const path = require('path');
const express = require('express');
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

const server = express();
server.use(express.static(__dirname + '/Styles'));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);
server.use(express.json());
server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

server.get('/tab', function (req, res) {
  res.render('./views/tab');
});

server.get('/productDetails', function (req, res) {
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

server.get('/scanProduct', function (req, res) {
  res.render('./views/scanProduct');
});

server.get('/viewProductDetail', function (req, res) {
  res.render('./views/viewProductDetail');
});

server.post('/save', upload.single('data'), function(req, res){
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

const port = process.env.port || process.env.PORT || 3978;

server.listen(port, () => 
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);