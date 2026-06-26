var express = require('express');
var router = express.Router();

/* GET home page. */
router.get('/', function (req, res, next) {
  const title = 'RAG Based Cosmos DB Semantic Search';
  res.render('index', { title });
});

module.exports = router;
