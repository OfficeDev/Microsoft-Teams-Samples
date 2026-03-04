document.addEventListener("DOMContentLoaded", function () {
    microsoftTeams.app.initialize().then(() => {
        microsoftTeams.app.getContext().then((context) => {
            microsoftTeams.app.notifySuccess();
        });
    });
});

// Navigation between tabs
function navigateBetweenTabs() {
    microsoftTeams.app.initialize().then(() => {
        microsoftTeams.app.getContext().then(() => {
            if (microsoftTeams.pages.currentApp.isSupported()) {
                microsoftTeams.pages.currentApp.navigateTo({ pageId: "tab_three", subPageId: "" })
                    .then(() => console.log("Navigation Successful"))
                    .catch((error) => console.log("Navigation Failed", error));
            } else {
                microsoftTeams.pages.navigateToApp({
                    appId: "<<External-App-Id>>", pageId: "tab_three"
                })
                    .then(() => console.log("Navigation Successful"))
                    .catch((error) => console.log("Navigation Failed", error));
            }
        });
    });
}

// Navigate to default tab
function onNavigateToDefaultTab() {
    microsoftTeams.app.initialize().then(() => {
        microsoftTeams.app.getContext().then(() => {
            if (microsoftTeams.pages.currentApp.isSupported()) {
                microsoftTeams.pages.currentApp.navigateToDefaultPage()
                    .then(() => console.log("This is Default Page"))
                    .catch((error) => console.log("Error", error));
            } else {
                microsoftTeams.pages.navigateToApp({ appId: "<<External-App-Id>>", pageId: "default_tab", subPageId: "" })
                    .then(() => console.log("Navigation Successful"))
                    .catch((error) => console.log("Error", error));
            }
        });
    });
}

// Back button navigation
function backButtonNavigation() {
    if (microsoftTeams.pages.backStack.isSupported()) {
        microsoftTeams.pages.backStack.navigateBack();
    } else {
        console.log("Capability is not supported");
    }
}