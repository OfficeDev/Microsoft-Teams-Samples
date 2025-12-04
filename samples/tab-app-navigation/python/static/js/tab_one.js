document.addEventListener("DOMContentLoaded", function () {
    microsoftTeams.app.initialize().then(() => {
        microsoftTeams.app.getContext().then(context => {
            microsoftTeams.app.notifySuccess();
        });
    });

    const navigateTabsBtn = document.getElementById("navigateTabsBtn");
    if (navigateTabsBtn) {
        navigateTabsBtn.addEventListener("click", function () {
            if (microsoftTeams.pages.currentApp.isSupported()) {  // ✅ No `.then()`
                microsoftTeams.pages.currentApp.navigateTo({ pageId: "tab_two", subPageId: "" })
                    .then(() => console.log("Navigation Successful"))
                    .catch(error => console.log("Navigation Failed", error));
            } else {
                microsoftTeams.pages.navigateToApp({
                    appId: "<<External-App-Id>>",
                    pageId: "tab_two"
                }).then(() => console.log("Navigation Successful"))
                  .catch(error => console.log("Navigation Failed", error));
            }
        });
    }

    const defaultTabBtn = document.getElementById("defaultTabBtn");
    if (defaultTabBtn) {
        defaultTabBtn.addEventListener("click", function () {
            if (microsoftTeams.pages.currentApp.isSupported()) {  // ✅ No `.then()`
                microsoftTeams.pages.currentApp.navigateToDefaultPage()
                    .then(() => console.log("Navigated to Default Page"))
                    .catch(error => console.log("Navigation Failed", error));
            } else {
                microsoftTeams.pages.navigateToApp({
                    appId: "<<External-App-Id>>",
                    pageId: "default_tab",
                    subPageId: ""
                }).then(() => console.log("Navigation Successful"))
                  .catch(error => console.log("Navigation Failed", error));
            }
        });
    }

    const backButton = document.getElementById("backButton");
    if (backButton) {
        backButton.addEventListener("click", function () {
            if (microsoftTeams.pages.backStack.isSupported()) {
                microsoftTeams.pages.backStack.navigateBack()
                    .then(() => console.log("Navigated Back"))
                    .catch(error => console.log("Back Navigation Failed", error));
            } else {
                console.log("Back navigation is not supported");
            }
        });
    }
});