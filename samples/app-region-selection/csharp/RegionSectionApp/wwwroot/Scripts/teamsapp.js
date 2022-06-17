(function () {
    'use strict';

    // Call the initialize API first
    microsoftTeams.app.initialize().then(() => {
        // Check the initial theme user chose and respect it
        microsoftTeams.app.getContext().then((context) => {
            if (context && context.app.theme) {
                setTheme(context.app.theme);
            }
        });

        // Handle theme changes
        microsoftTeams.app.registerOnThemeChangeHandler(function (theme) {
            setTheme(theme);
        });

        // Save configuration changes
        microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
            // Let the Microsoft Teams platform know what you want to load based on
            // what the user configured on this page
            microsoftTeams.pages.config.setConfig({
                contentUrl: createTabUrl(), // Mandatory parameter
                entityId: createTabUrl() // Mandatory parameter
            });

            // Tells Microsoft Teams platform that we are done saving our settings. Microsoft Teams waits
            // for the app to call this API before it dismisses the dialog. If the wait times out, you will
            // see an error indicating that the configuration settings could not be saved.
            saveEvent.notifySuccess();
        });
    });

    // Logic to let the user configure what they want to see in the tab being loaded
    document.addEventListener('DOMContentLoaded', function () {
        var domainChoice = document.getElementById('domainChoice');
        if (domainChoice) {
            domainChoice.onchange = function () {
                var selectedDomain = this[this.selectedIndex].value;
                // This API tells Microsoft Teams to enable the 'Save' button. Since Microsoft Teams always assumes
                // an initial invalid state, without this call the 'Save' button will never be enabled.
                microsoftTeams.pages.config.setValidityState(selectedDomain !== '' && selectedDomain !== null && selectedDomain!==undefined);
            };
        }
    });

    // Set the desired theme
    function setTheme(theme) {
        if (theme) {
            // Possible values for theme: 'default', 'light', 'dark' and 'contrast'
            document.body.className = 'theme-' + (theme === 'default' ? 'light' : theme);
        }
    }

    // Create the URL that Microsoft Teams will load in the tab. You can compose any URL even with query strings.
    function createTabUrl() {
        var domainChoice = document.getElementById('domainChoice');
        var selectedDomain = domainChoice[domainChoice.selectedIndex].value;
        var _url = window.location.protocol + '//' + window.location.host + '/welcome?selectedDomain=' + selectedDomain;
        return _url;
    }
})();
