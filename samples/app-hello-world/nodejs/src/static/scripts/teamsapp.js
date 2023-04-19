(async function () {
    'use strict';

    // Call the initialize API first
    await microsoftTeams.app.initialize();

    // Check the initial theme user chose and respect it
    microsoftTeams.getContext(function (context) {
        if (context && context.theme) {
            setTheme(context.theme);
        }
    });
    
    // Handle theme changes
    microsoftTeams.registerOnThemeChangeHandler(function (theme) {
        setTheme(theme);
    });

    // Handle save for configurable tab.
    microsoftTeams.pages.config.registerOnSaveHandler((saveEvent) => {
        const configPromise = microsoftTeams.pages.config.setConfig({
            websiteUrl: createTabUrl(),
            contentUrl: createTabUrl(),
            entityId: createTabUrl(),
        });
        configPromise.
            then((result) => { saveEvent.notifySuccess() }).
            catch((error) => { saveEvent.notifyFailure("failure message") });
    });

    // Logic to let the user configure what they want to see in the tab being loaded
    var tabChoice = document.getElementById('tabChoice');
    tabChoice.onchange = function () {
        var selectedTab = tabChoice[tabChoice.selectedIndex].value;
        microsoftTeams.pages.config.setValidityState(
            selectedTab === 'first' || selectedTab === 'second'
        );
    }

    // Set the desired theme
    function setTheme(theme) {
        if (theme) {
            // Possible values for theme: 'default', 'light', 'dark' and 'contrast'
            document.body.className =
                'theme-' + (theme === 'default' ? 'light' : theme);
        }
    }

    // Create the URL that Microsoft Teams will load in the tab. You can compose any URL even with query strings.
    function createTabUrl() {
        var tabChoice = document.getElementById('tabChoice');
        var selectedTab = tabChoice[tabChoice.selectedIndex].value;

        return (
            window.location.protocol +
            '//' +
            window.location.host +
            '/' +
            selectedTab
        );
    }
})();
