// An authentication service that uses the MSAL.js library to sign in users with
// either an AAD or MSA account. This leverages the AAD v2 endpoint.
class MsalAuthService {
    constructor(clientId, applicationIdUri) {
        this.api = applicationIdUri;

        this.app = new msal.PublicClientApplication({
            auth: {
                clientId: clientId,
                redirectUri: `${window.location.origin}/Home/BrowserRedirect`,
            },
        });
    }

    isCallback() {
        return this.app.handleRedirectPromise().then((authResponse) => {
            if (authResponse) {
                this.app.setActiveAccount(authResponse.account);
                return true;
            } else {
                return false;
            }
        });
    }

    login() {
        // Configure all the scopes that this app needs
        const loginScopes = [
            "openid",
            "email",
            "profile",
            "offline_access",
            "User.Read"
        ];

        const authRequest = {
            scopes: loginScopes,
            prompt: "select_account",
        };

        if (window.navigator.standalone) {
            return this.app.loginRedirect(authRequest);
        } else {
            // This method is called for browser auth only outside teams
            return this.app.loginPopup(authRequest).then((authResponse) => {
                this.app.setActiveAccount(authResponse.account);
                
                return authResponse.account;
            });
        }
    }

    logout() {
        this.app.logout();
    }

    getUser() {
        let activeAccount = this.app.getActiveAccount();
        if (!activeAccount) {
            const allAccounts = this.app.getAllAccounts();
            if (allAccounts.length === 1) {
                this.app.setActiveAccount(allAccounts[0]);
                activeAccount = allAccounts[0];
            }
        }

        return Promise.resolve(activeAccount);
    }

    getToken() {
        const scopes = [this.api];
        
        return this.app
            .acquireTokenSilent({ account: this.app.getActiveAccount() }) // This method is called for browser auth outside only Teams app.
            .then((authResponse) => authResponse.accessToken)
            .catch((error) => {
                if (error.errorMessage.indexOf("interaction_required") >= 0) {
                    return this.app
                        .acquireTokenPopup({ scopes })
                        .then((authResponse) => authResponse.accessToken);
                } else {
                    return Promise.reject(error);
                }
            });
    }

    getUserInfo(principalName) {
        this.getToken().then((token) => {
            if (principalName) {
                let graphUrl = "https://graph.microsoft.com/v1.0/users/" + principalName;

                $.ajax({
                    url: graphUrl,
                    type: "GET",
                    beforeSend: function (request) {
                        request.setRequestHeader("Authorization", `Bearer ${token}`);
                    },
                    success: function (profile) {
                        let profileDiv = $("#divGraphProfile");
                        profileDiv.empty();
                        
                        $("<div>")
                            .append($("<h2>").text(`Welcome ${profile["displayName"]},`))
                            .append($("<h3>").text(`Here is your profile details:`))
                            .appendTo(profileDiv);

                        for (let key in profile) {
                            if ((key[0] !== "@") && profile[key]) {
                                $("<div>")
                                    .append($("<span>").text(key + ": "))
                                    .append($("<span>").text(profile[key]))
                                    .appendTo(profileDiv);
                            }
                        }

                        $("<div>")
                            .append($("<button class=\"browser-button\" onclick=\"logout()\">").text(`Logout`))
                            .appendTo(profileDiv);

                        $("#divGraphProfile").show();
                    },
                    error: function (error) {
                        console.log("Failed");
                        console.log(error);
                    },
                    complete: function (data) {
                    }
                });
            }
        });
    }
}