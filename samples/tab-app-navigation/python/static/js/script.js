document.addEventListener("DOMContentLoaded", function () {
    microsoftTeams.app.initialize().then(() => {
        microsoftTeams.app.getContext().then(context => {
            microsoftTeams.app.notifySuccess();
        });
    });

    document.getElementById("navigateBtn").addEventListener("click", function () {
        if (microsoftTeams.pages.currentApp.isSupported()) {  
            microsoftTeams.pages.currentApp.navigateTo({ pageId: "tab_One", subPageId: "" })
                .then(() => console.log("Navigation Successful"))
                .catch(error => console.log("Navigation Failed", error));
        } else {
            microsoftTeams.pages.navigateToApp({
                appId: "<<External-App-Id>>",
                pageId: "tab_one"
            }).then(() => console.log("Navigation Successful"))
              .catch(error => console.log("Navigation Failed", error));
        }
    });
});