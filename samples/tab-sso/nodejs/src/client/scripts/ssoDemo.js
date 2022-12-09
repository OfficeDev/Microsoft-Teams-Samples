function ssoAuth() {
    'use strict';
    microsoftTeams.app.initialize();

    // 1. Get auth token
    // Ask Teams to get us a token from AAD
    function getClientSideToken() {

        return new Promise((resolve, reject) => {
            display("1. Get auth token from Microsoft Teams");
            
            microsoftTeams.authentication.getAuthToken().then((result) => {
                display(result);

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
                fetch('/getProfileOnBehalfOf', {
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
                        const profile = responseJson;

                        resolve(profile);
                    }
                });
            });
        });
    }

    // 3. Get the server side token and use it to call the Graph API
    function useServerSideToken(data) {

        display("2. Call https://graph.microsoft.com/v1.0/me/ with the server side token");

        return display(JSON.stringify(data, undefined, 4), 'pre');
    }

    // Show the consent pop-up
    function requestConsent() {
        return new Promise((resolve, reject) => {
            microsoftTeams.authentication.authenticate({
                url: window.location.origin + "/auth-start",
                width: 600,
                height: 535
            })
            .then((result) => {
                let tokenData = result;
                resolve(tokenData);
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

    // In-line code
    $(document).ready(function () {
        microsoftTeams.app.initialize().then(() => {
            getClientSideToken()
                .then((clientSideToken) => {
                    return getServerSideToken(clientSideToken);
                })
                .then((profile) => {
                    return useServerSideToken(profile);
                })
                .catch((error) => {
                    if (error === "invalid_grant") {
                        display(`Error: ${error} - user or admin consent required`);

                        // Display in-line button so user can consent
                        let button = display("Consent", "button");
                        button.onclick = (() => {
                            requestConsent()
                                .then((result) => {
                                    // Consent succeeded
                                    display(`Consent succeeded`);

                                    // offer to refresh the page
                                    button.disabled = true;
                                    let refreshButton = display("Refresh page", "button");
                                    refreshButton.onclick = (() => { window.location.reload(); });
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
        }).catch((error) => {
            console.error(error);
        });
    });

}