// Initialize the teams js sdk
$(document).ready(function () {
    microsoftTeams.app.initialize().then(() => {
    });
});

// Function to fetch user's google profile details
function googleAuth() {
    getGoogleIdToken()
        .then((result) => {
            return getGoogleServerSideToken(result);
        })
        .catch((error) => {
            console.log(error);
        });
}

// Exchange id token with access token at server side and fetch profile details
function getGoogleServerSideToken(clientSideToken) {
    return new Promise((resolve, reject) => {
        microsoftTeams.app.getContext().then((context) => {
            $.ajax({
                type: 'POST',
                url: '/getGoogleAccessToken',
                dataType: 'json',
                data: {
                    'idToken': clientSideToken,
                },
                success: function (responseJson) {
                    var googleProfile = JSON.parse(responseJson);
                    $("#userImgGoogle").attr("src", googleProfile.picture);
                    $("#gname").empty();
                    $("#gemail").empty();
                    $("#gname").append("<b>Name: </b>" + googleProfile.name);
                    $("#gemail").append("<b>Email: </b>" + googleProfile.email);
                    $("#googlelogin").hide();
                },
                error: function (xhr, textStatus, errorThrown) {
                    console.log("textStatus: " + textStatus + ", errorThrown:" + errorThrown);
                }
            });
        });
    });
}