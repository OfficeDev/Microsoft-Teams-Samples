const { getAccessToken } = require('../services/authService');
const { SimpleGraphClient } = require('../models/simpleGraphClient');

const getToken = async (req, res) => {
    const token = await getAccessToken(req);
    res.json(token);
};

const getUserProfile = async (req, res) => {
    const token = await getAccessToken(req);
    const graphClient = new SimpleGraphClient(token);
    const profile = await graphClient.getMyProfile();
    res.json(profile);
};

const getUserPhoto = async (req, res) => {
    const token = await getAccessToken(req);
    const graphClient = new SimpleGraphClient(token);
    const photo = await graphClient.getPhotoAsync();
    res.json(photo);
};

module.exports = {
    getToken,
    getUserProfile,
    getUserPhoto
};
