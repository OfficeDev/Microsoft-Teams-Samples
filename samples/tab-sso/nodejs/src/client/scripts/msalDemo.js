var auth;

function msalAuth(authService) {
    auth = authService;
    document.getElementById("browser-signin-container").hidden = false;
    document.getElementById("browser-signin-text").hidden = true;
    authService
        .isCallback()
        .then((isCallback) => {
            if (!isCallback) {
                authService
                    .getUser()
                    .then((user) => {
                        // Signed in the user automatically; we're ready to go
                        setUserSignedIn(true);
                        getMyProfile(user.localAccountId);
                    })
                    .catch(() => {
                        setUserSignedIn(false);
                        // Failed to sign in the user automatically; show login screen
                        console.log("Failed")
                    });
            }
        })
        .catch((error) => {
            // Encountered error during redirect login flow; show error screen
            console.log(error);
        });
  }

  function loginUser() {
    auth
        .login()
        .then((user) => {
            if (user) {
                setUserSignedIn(true);
                getMyProfile(user.localAccountId);
            } else {
                setUserSignedIn(false);
            }
        })
        .catch((err) => {
            setUserSignedIn(false);
        });
}

function logout() {
    auth.logout();
}

function getMyProfile(userId) {
    setUserSignedIn(true);
    auth.getUserInfo(userId);
}

function setUserSignedIn(isUserSignedIn) {
  console.log(document.getElementById("browser-login"));
    document.getElementById("browser-login").hidden = isUserSignedIn;
}