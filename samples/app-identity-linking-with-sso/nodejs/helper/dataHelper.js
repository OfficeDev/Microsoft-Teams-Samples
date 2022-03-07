const { SimpleGraphClient } = require('../simpleGraphClient');
const axios = require('axios');

// Method to get facebook user data.
async function getFacebookUserData(access_token) {
    const { data } = await axios({
      url: 'https://graph.facebook.com/v2.6/me',
      method: 'get',
      params: {
        fields: ['name', 'picture', 'id'].join(','),
        access_token: access_token,
      },
    });

    return data;
  };

// Method to get AAD data.
async function getAADUserData(token) {
  const client = new SimpleGraphClient(token);
  const myDetails = await client.getMeAsync();
  var imageString = "";
  var img2 = "";

  if (myDetails != null) {
    var userImage = await client.getUserPhoto();
    await userImage.arrayBuffer().then(result => {
      imageString = Buffer.from(result).toString('base64');
      img2 = "data:image/png;base64," + imageString;
    }).catch(error => {
      console.log(error)
    });
  }

  return {
    myDetails: myDetails,
    photo: img2
  }
}

// Method to get google user data.
async function getGoogleUserData(access_token) {
  const data = await axios.get('https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls', {
    headers: {
      "Authorization": `Bearer ${access_token}`,
    }
  })

  return data.data;
};

module.exports = {
  getGoogleUserData,
  getAADUserData,
  getFacebookUserData
}