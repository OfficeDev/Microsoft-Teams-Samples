document.addEventListener("DOMContentLoaded", function () {
    microsoftTeams.app.initialize().then(() => {
        microsoftTeams.app.getContext().then(() => {
            microsoftTeams.app.notifySuccess();
        });
    });
});

// Navigate to default tab
function onNavigateToDefaultTab() {
    if (microsoftTeams.pages.currentApp.isSupported()) {  // ✅ No `.then()`
        microsoftTeams.pages.currentApp.navigateToDefaultPage()
            .then(() => console.log("Navigated to Default Page"))
            .catch((error) => console.log("Navigation Failed", error));
    } else {
        console.log("Capability is not supported");
        microsoftTeams.pages.navigateToApp({
            appId: "<<External-App-Id>>", 
            pageId: "default_tab", 
            subPageId: ""
        })
        .then(() => console.log("Navigation Successful"))
        .catch((error) => console.log("Navigation Failed", error));
    }
}

// Back button navigation
function backButtonNavigation() {
    if (microsoftTeams.pages.backStack.isSupported()) {  // ✅ No `.then()`
        microsoftTeams.pages.backStack.navigateBack()
            .then(() => console.log("Navigated Back"))
            .catch((error) => console.log("Back Navigation Failed", error));
    } else {
        console.log("Back navigation is not supported");
    }
}