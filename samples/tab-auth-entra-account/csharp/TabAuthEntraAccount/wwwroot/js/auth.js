// Initialize the teams js sdk
$(document).ready(function () {
    microsoftTeams.app.initialize().then(() => {
        console.log("Microsoft Teams SDK initialized successfully");
    });
});

// Function to fetch user's Microsoft Graph profile details
function googleAuth() {
    // Check if we have the new comprehensive UI functions
    if (typeof showLoading === 'function') {
        showLoading(true);
    }
    if (typeof hideError === 'function') {
        hideError();
    }
    
    getGoogleIdToken()
        .then((result) => {
            return getGoogleServerSideToken(result);
        })
        .catch((error) => {
            console.log("Authentication error:", error);
            if (typeof showError === 'function') {
                showError("Authentication failed: " + error);
            }
            if (typeof showLoading === 'function') {
                showLoading(false);
            }
        });
}

// Exchange id token with access token at server side and fetch comprehensive profile details
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
                    try {
                        var profile = JSON.parse(responseJson);
                        console.log("Received comprehensive profile data:", profile);
                        
                        // Check if we have the new comprehensive display functions
                        if (typeof populateAllProfileData === 'function') {
                            // New comprehensive display
                            populateAllProfileData(profile);
                            if (typeof showProfileCard === 'function') {
                                showProfileCard();
                            }
                        } else {
                            // Fallback to simple display for backwards compatibility
                            displaySimpleProfile(profile);
                        }
                        
                        if (typeof showLoading === 'function') {
                            showLoading(false);
                        }
                        
                        resolve(profile);
                    } catch (e) {
                        console.error("Error parsing profile data:", e);
                        if (typeof showError === 'function') {
                            showError("Error parsing profile data: " + e.message);
                        }
                        if (typeof showLoading === 'function') {
                            showLoading(false);
                        }
                        reject(e);
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    var errorMessage = "Request failed - textStatus: " + textStatus + ", errorThrown:" + errorThrown;
                    console.log(errorMessage);
                    
                    if (typeof showError === 'function') {
                        showError("Failed to fetch profile data. Please try again.");
                    }
                    if (typeof showLoading === 'function') {
                        showLoading(false);
                    }
                    
                    reject(new Error(errorMessage));
                }
            });
        }).catch((error) => {
            console.error("Failed to get Teams context:", error);
            if (typeof showError === 'function') {
                showError("Failed to get Teams context: " + error.message);
            }
            if (typeof showLoading === 'function') {
                showLoading(false);
            }
            reject(error);
        });
    });
}

// Simple profile display for backwards compatibility (legacy UI) - PERSONAL INFO ONLY
function displaySimpleProfile(profile) {
    console.log("Using legacy simple profile display - PERSONAL INFO ONLY");
    
    // Update basic info (backwards compatibility) - only name and email
    if ($("#gname").length > 0) {
        $("#gname").empty().append("<b>Name: </b>" + (profile.displayName || profile.name || "Not Available"));
    }
    
    if ($("#gemail").length > 0) {
        $("#gemail").empty().append("<b>Email: </b>" + (profile.email || profile.userPrincipalName || "Not Available"));
    }
    
    // Hide login button
    if ($("#googlelogin").length > 0) {
        $("#googlelogin").hide();
    }
    
    // DO NOT populate any other fields - Personal Information only
    console.log("Profile display limited to name and email only");
}