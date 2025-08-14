document.addEventListener("DOMContentLoaded", function () {
    microsoftTeams.app.initialize().then(() => {
        microsoftTeams.app.notifySuccess();

        microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
            microsoftTeams.pages.config.setConfig({
                entityId: "AppInstance_" + Math.floor(Math.random() * 100 + 1),
                contentUrl: `${window.location.origin}/default_tab`,
                suggestedTabName: "Default-Tab",
                websiteUrl: `${window.location.origin}/default_tab`,
            });

            saveEvent.notifySuccess();
        });

        microsoftTeams.pages.config.setValidityState(true);
    });
});