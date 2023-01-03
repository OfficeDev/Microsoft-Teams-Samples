let button;
microsoftTeams.app.initialize();

getClientSideToken()
    .then((clientSideToken) => {
        return getServerSideToken(clientSideToken);
    })
    .then((serverSideToken) => {
        return useServerSideToken(serverSideToken);
    })
    .catch((error) => {
        if (error === "invalid_grant") {
            display(`Error: ${error} - user or admin consent required`);
            // Display in-line button so user can consent
            button = display("Consent", "button");
            button.onclick = (() => {
                requestConsent()
                    .then((result) => {
                        // Consent succeeded - use the token we got back
                        getClientSideToken()
                        .then((clientSideToken) => {
                            return getServerSideToken(clientSideToken);
                        })
                        .then((serverSideToken) => {
                            return useServerSideToken(serverSideToken);
                        })
                    })
                    .catch((error) => {
                        display(`ERROR ${error}`);
                        // Consent failed - offer to refresh the page
                        button.disabled = true;
                        let refreshButton = display("Refresh page", "button");
                        refreshButton.onclick = (() => { window.location.reload(); });
                    });
            });
        } else {
            // Something else went wrong
            display(`Error from web service: ${error}`);
        }
    });

function getClientSideToken() {
    return new Promise((resolve, reject) => {

        microsoftTeams.authentication.getAuthToken().then((result) => {
            console.log(result);
                resolve(result);
        }).catch((error) => {
            reject("Error getting token: " + error);
        });
    });
}

// 2. Exchange that token for a token with the required permissions
//    using the web service (see /auth/token handler in app.js)
function getServerSideToken(clientSideToken) {

    return new Promise((resolve, reject) => {

        microsoftTeams.app.getContext().then((context) => {

            fetch('/auth/token', {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    'tid': context.user.tenant.id,
                    'token': clientSideToken
                }),
                mode: 'cors',
                cache: 'default'
            })
                .then((response) => {
                    console.log(response);
                    if (response.ok) {
                        return response.json();
                    } else {
                        reject(response.error);
                    }
                })
                .then((responseJson) => {
                    if (responseJson.error) {
                        reject(responseJson.error);
                    } else {
                        serverSideToken = responseJson;
                        localStorage.setItem("accessToken", serverSideToken);
                        $("#createGroupChat").show();
                        if (button)
                        button.disabled = true;
                        resolve(serverSideToken);
                    }
                });
        });
    });
}

// 3. Get the server side token and use it to call the Graph API
function useServerSideToken(data) {

    return fetch("https://graph.microsoft.com/v1.0/me/",
        {
            method: 'GET',
            headers: {
                "accept": "application/json",
                "authorization": "bearer " + data
            },
            mode: 'cors',
            cache: 'default'
        })
        .then((response) => {
            if (response.ok) {
                return response.json();
            } else {
                throw (`Error ${response.status}: ${response.statusText}`);
            }
        })
}

// Show the consent pop-up
function requestConsent() {
    return new Promise((resolve, reject) => {
        microsoftTeams.authentication.authenticate({
        url: window.location.origin + "/auth/auth-start",
        width: 600,
        height: 535}).then((result) => {
            let data = localStorage.getItem(result);
            localStorage.removeItem(result);
            resolve(data);
        }).catch((reason) => {
            reject(JSON.stringify(reason));
        });
    });
}

// Add text to the display in a <p> or other HTML element
function display(text, elementTag) {
    var logDiv = document.getElementById('logs');
    var p = document.createElement(elementTag ? elementTag : "p");
    p.innerText = text;
    logDiv.append(p);
    console.log("ssoDemo: " + text);
    return p;
}
