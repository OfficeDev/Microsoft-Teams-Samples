let accessToken;

$(document).ready(function () {
    microsoftTeams.initialize();

    getClientSideToken()
        .then((clientSideToken) => {
            return getServerSideToken(clientSideToken);
        })
        .catch((error) => {
            if (error === "invalid_grant") {
                // Display in-line button so user can consent
                $("#divError").text("Error while exchanging for Server token - invalid_grant - User or admin consent is required.");
                $("#divError").show();
                $("#consent").show();
                $("#adaptiveBtn").hide();
            } else {
                // Display in-line button so user can consent
                $("#divError").text("Error while exchanging for Server token - invalid_grant - User or admin consent is required.");
                $("#divError").show();
                $("#consent").show();
                $("#adaptiveBtn").hide();
            }
        });
});

function requestConsent() {
    getToken()
        .then(data => {
            getClientSideToken()
                .then((clientSideToken) => {
                    return getServerSideToken(clientSideToken);
                })
        });
}

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

function getServerSideToken(clientSideToken) {
    return new Promise((resolve, reject) => {
        microsoftTeams.getContext((context) => {
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
                    if (IsValidJSONString(responseJson)) {
                        if (JSON.parse(responseJson).error)
                            reject(JSON.parse(responseJson).error);
                    } else if (responseJson) {
                        accessToken = responseJson;
                        localStorage.setItem("accessToken", accessToken);
                    }
                });
        });
    });
}

function IsValidJSONString(str) {
    try {
        JSON.parse(str);
    } catch (e) {
        return false;
    }
    return true;
}