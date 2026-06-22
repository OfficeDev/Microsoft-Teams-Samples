handlePageLoad = async () => {

    try {
        window.localStorage.removeItem("isTeams");
        await this.initialiseTeams();
        this.inTeams = true;
    }
    catch {
        this.inTeams = false;
    }

    if (this.inTeams) {
        await this.signInWithTeams();
    }
    else {
        outsideTeamsSignIn();
    }
}

initialiseTeams = () => {
    let rejectPromise = null;
    let timeout = null;

    const promise = new Promise((resolve, reject) => {
        rejectPromise = reject;
        microsoftTeams.initialize(() => {
            window.clearTimeout(timeout);
            resolve(true);
        }
        )
    });
    //todo: try and improve this if possible. At present this function will return that the page is not open in Teams
    //based on the MicrosoftTeams.initialize function timing out within 2 seconds

    timeout = window.setTimeout(() => {
        rejectPromise("Teams Initialise Timeout");
    }, 2000);

    return promise;
}


signInWithTeams = async () => {
    try {

        const result = await this.getTeamsToken();
        let accessToken = window.localStorage.getItem("userToken");
        return
    }
    catch (error) {
        console.log("Teams SSO Error: " + error);
        if (typeof error === "string" && error.toLowerCase() == "resourcedisabled") {
            console.log("Resource Disabled - probably something wrong with your manifest or Azure AD App Reg config");
            $("#teamsConsentButton").show();
            return
        }

        else {
            console.warn("error... but, it wasn't resource disabled....")
            $("#teamsConsentButton").show();
            return
        }
    }
}

getTeamsToken = () => {
    window.localStorage.setItem("isTeams", "yes");
    return new Promise((resolve, reject) => {
        microsoftTeams.authentication.getAuthToken({
            successCallback: (token) => {
                window.localStorage.setItem("userToken", token);
                resolve(token);
            },
            failureCallback: (reason) => {
                reject(reason);
            }
        })
    });
}

function teamsFallbackAuth() {

    microsoftTeams.authentication.authenticate({
        url: window.location.origin + "/StaticViews/Login.html",
        width: 600,
        height: 535,
        successCallback: function (result) {
        },
        failureCallback: function (reason) {
            handleAuthError(reason);
        }
    });
}

function outsideTeamsSignIn() {
    let accessToken = window.localStorage.getItem("userToken");
    if (!accessToken) {
        authRedirect();
    }
}

function authRedirect() {
    //window.localStorage.setItem("sourceHostname", location.protocol + "//" + location.hostname + ":" + location.port + "/StaticViews/OpenPositionsPersonalTab.html");
    window.localStorage.setItem("sourceHostname", window.location.origin + window.location.pathname + window.location.search);
    console.log("redirectURL will be: " + window.localStorage.getItem("sourceHostname"));
    window.location.assign(window.location.origin + "/StaticViews/Login.html");
}

function getInfoFromToken(accessToken) {
    var token = accessToken;
    var decoded = jwt_decode(token);
    let preferredusername = decoded.preferred_username
    window.localStorage.setItem("username", preferredusername);
}