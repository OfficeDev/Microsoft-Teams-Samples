﻿let accessToken;

$(document).ready(function () {
    microsoftTeams.app.initialize().then(() => {
        getClientSideToken()
        .then((clientsidetoken) => {
            console.log("clientsidetoken: " + clientsidetoken);
            return getServerSideToken(clientsidetoken);
        })
        .catch((error) => {
            console.log(error);
            if (error === "invalid_grant") {
                // display in-line button so user can consent
                $("#diverror").text("error while exchanging for server token - invalid_grant - user or admin consent is required.");
                $("#diverror").show();
                $("#consent").show();
            } else {
                console.log("authentication failed. something went wrong");
            }
        });
    });
});

function requestConsent() {
    getToken()
        .then(data => {
            $("#consent").hide();
            $("#divError").hide();
            getClientSideToken()
                .then((clientSideToken) => {
                    return getServerSideToken(clientSideToken);
                })
                .catch((error) => {
                    console.log(error);
                });
        });
}

function getToken() {
    return new Promise((resolve, reject) => {
        microsoftTeams.authentication.authenticate({
            url: window.location.origin + "/Auth/Start",
            width: 600,
            height: 535,
        }).then((result) => {
            resolve(result);
        }).catch((error) => {
            reject(error);
        });
    });
}

function getClientSideToken() {

    return new Promise((resolve, reject) => {
        microsoftTeams.authentication.getAuthToken().then((result) => {
            resolve(result);
        }).catch((error) => {
            console.log("error" + error);
            reject("Error getting token: " + error);
        });
    });

}

function getServerSideToken(clientSideToken) {
    return new Promise((resolve, reject) => {
        microsoftTeams.app.getContext().then((context) => {
            {
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
                            if (responseJson == "invalid_grant") {
                                console.log("error" + responseJson);
                                $("#diverror").text("error while exchanging for server token - invalid_grant - user or admin consent is required.");
                                $("#diverror").show();
                                $("#consent").show();
                            }
                            else {
                                accessToken = responseJson;
                                getUserInfo(context.user.userPrincipalName);
                                getPhotoAsync(accessToken);
                            }
                        }
                    });
            }
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

function getUserInfo(principalName) {
    if (principalName) {
        let graphUrl = "https://graph.microsoft.com/v1.0/users/" + principalName;
        $.ajax({
            url: graphUrl,
            type: "GET",
            beforeSend: function (request) {
                request.setRequestHeader("Authorization", `Bearer ${accessToken}`);
            },
            success: function (profile) {
                let profileDiv = $("#divGraphProfile");
                profileDiv.empty();
                for (let key in profile) {
                    if ((key[0] !== "@") && profile[key]) {
                        $("<div>")
                            .append($("<b>").text(key + ": "))
                            .append($("<span>").text(profile[key]))
                            .appendTo(profileDiv);
                    }
                }
                $("#divGraphProfile").show();
            },
            error: function () {
                console.log("Failed");
            },
            complete: function (data) {
            }
        });
    }
}

// Gets the user's photo
function getPhotoAsync(token) {
    let graphPhotoEndpoint = 'https://graph.microsoft.com/v1.0/me/photos/240x240/$value';
    let request = new XMLHttpRequest();
    request.open("GET", graphPhotoEndpoint, true);
    request.setRequestHeader("Authorization", `Bearer ${token}`);
    request.setRequestHeader("Content-Type", "image/png");
    request.responseType = "blob";
    request.onload = function (oEvent) {
        let imageBlob = request.response;
        if (imageBlob) {
            let urlCreater = window.URL || window.webkitURL;
            let imgUrl = urlCreater.createObjectURL(imageBlob);
            $("#userPhoto").attr('src', imgUrl);
            $("#userPhoto").show();
        }
    };
    request.send();
}