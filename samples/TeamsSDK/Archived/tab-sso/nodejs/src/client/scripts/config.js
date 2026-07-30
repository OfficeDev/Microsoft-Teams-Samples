(function () {
    microsoftTeams.app.initialize().then(() => {
        // Save configuration changes
        microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {

            var tabUrl = window.location.protocol +
                '//' + window.location.host + '/ssoDemo/?inTeams=true';

            // Let the Microsoft Teams platform know what you want to load based on
            // what the user configured on this page
            microsoftTeams.pages.config.setConfig({
                contentUrl: tabUrl, // Mandatory parameter
                entityId: tabUrl    // Mandatory parameter
            });

            // Tells Microsoft Teams platform that we are done saving our pages.config. Microsoft Teams waits
            // for the app to call this API before it dismisses the dialog. If the wait times out, you will
            // see an error indicating that the configuration pages.config could not be saved.
            saveEvent.notifySuccess();
        });

        microsoftTeams.pages.config.setValidityState(true);
    });
})();