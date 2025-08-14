let idToken;
let accessToken;

$(document).ready(function () {
    microsoftTeams.app.initialize().then(() => {
    getClientSideToken()
        .then((clientSideToken) => {
            return getServerSideToken(clientSideToken);
        })
    });
});

function getToken() {
    return new Promise((resolve, reject) => {
        microsoftTeams.authentication.authenticate({
            url: window.location.origin + "/Auth/Start",
            width: 600,
            height: 535,
            successCallback: result => {
                resolve(result);
            },
            failureCallback: reason => {
                reject(reason);
            }
        });
    });
}

function getClientSideToken() {
    return new Promise((resolve, reject) => {
        microsoftTeams.authentication.getAuthToken({
            successCallback: (result) => {
                resolve(result);
            },
            failureCallback: function (error) {
                reject("Error getting token: " + error);
            }
        });
    });
}

function grantConsent() {
    getToken()
        .then(data => {
            $("#consent").hide();
            $("#grant-consent").hide();

            getClientSideToken()
                .then((clientSideToken) => {
                    return getServerSideToken(clientSideToken);
                });
        });
}

function getServerSideToken(clientSideToken) {
    return new Promise((resolve, reject) => {
        microsoftTeams.app.getContext().then((context) => {
            var scopes = ["https://graph.microsoft.com/User.Read"];
            fetch('/GetUserAccessToken', {
                method: 'get',
                headers: {
                    "Content-Type": "application/text",
                    "Authorization": "Bearer " + clientSideToken
                },
                cache: 'default'
            })
                .then((response) => {
                    if (response.ok) {
                        return response.text();
                    } else {
                        reject(response.error);
                    }
                })
                .then((responseJson) => {
                    if (responseJson == "") {
                        $("#send-notification").hide();
                        $("#consent").show();
                        $("#grant-consent").show();
                    } else {
                        accessToken = responseJson;
                        localStorage.setItem("accessToken", accessToken);
                    }
                });
        });
    });
}