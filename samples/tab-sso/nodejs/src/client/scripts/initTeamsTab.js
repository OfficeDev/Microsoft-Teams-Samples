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
    });

    // Set the desired theme
    function setTheme(theme) {
        if (theme) {
            // Possible values for theme: 'default', 'light', 'dark' and 'contrast'
            document.body.className = 'theme-' + (theme === 'default' ? 'light' : theme);
        }
    }

})();
