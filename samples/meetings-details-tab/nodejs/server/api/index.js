var express = require('express');
var router = express.Router();
const home = require('./homeController');
router.use('/v1', require('./v1'));
router.post('/messages', require('./botController'));
router.post('/sendAgenda', home.sendAgenda);
router.get('/getAgendaList', home.getAgendaList);
router.post('/setAgendaList', home.setAgendaList);

module.exports = router;