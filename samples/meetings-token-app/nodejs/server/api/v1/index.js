var express = require('express');
var router = express.Router();

// Home page route.
router.get('/', function (req, res) {
    res.json({ message: "Hello from Server v1!" });
  })
  
  // About page route.
  router.get('/about', function (req, res) {
    res.json({ message: "About Page v1!" });
  })
  
  module.exports = router;