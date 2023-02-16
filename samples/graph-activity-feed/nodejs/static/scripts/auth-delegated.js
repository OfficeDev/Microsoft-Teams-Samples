
// method invoked on sso authentication.
function login() {
    microsoftTeams.app.initialize();

    getClientSideToken()
        .then((clientSideToken) => {
            return getServerSideToken(clientSideToken);
        })
        .catch((error) => {
            if (error === "invalid_grant") {
                console.log(`Error: ${error} - user or admin consent required`);
                // Display in-line button so user can consent
                $("#consent").show();

            } else {
                // Something else went wrong
                console.log(`Error from web service: ${error}`);
                $("#consent").show();
            }
        });
}

// Get auth token
// Ask Teams to get us a token from AAD
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

// Exchange that token for a token with the required permissions
// using the web service (see /auth/token handler in app.js)
function getServerSideToken(clientSideToken) {
    return new Promise((resolve, reject) => {
        microsoftTeams.app.getContext().then((context) => {
            console.log(context);
            fetch('/auth/token', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({
                    tid: context.user.tenant.id,
                    token: clientSideToken
                }),
            })
                .then((response) => {
                    if (response.ok) {
                        return response.text();
                    } else {
                        reject(response.error);
                    }
                })
                .then((responseJson) => {
                    if (responseJson.error) {
                        reject(responseJson.error);
                    }
                    else {
                        $("#login").hide();
                        $("#feed-container").show();
                        resolve();
                    }
                });
        });
    });
}

// Show the consent pop-up
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
            url: window.location.origin + "/auth-start",
            width: 600,
            height: 535,
        }).then((result) => {
            resolve(result);
        }).catch((error) => {
            reject(error);
        });
    });
}