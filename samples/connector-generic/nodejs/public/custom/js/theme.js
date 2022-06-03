/**
 * Class for managing Microsoft Teams themes
 * idea borrowed from the Dizz: https://github.com/richdizz/Microsoft-Teams-Tab-Themes/blob/master/app/config.html
 * Updated on 4/27/17 to use a hierarchical styles approach with a simple stylesheet
 */
var TeamsTheme = (function () {
    function TeamsTheme() {
    }
    /**
     * Set up themes on a page
     */
    TeamsTheme.fix = function (context) {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.registerOnThemeChangeHandler(TeamsTheme.themeChanged);
            if (context) {
                TeamsTheme.themeChanged(context.app.theme);
            }
            else {
                microsoftTeams.app.getContext().then((context) => {
                    TeamsTheme.themeChanged(context.app.theme);
                });
            }
        });
    };
    /**
     * Manages theme changes
     * @param theme default|contrast|dark
     */
    TeamsTheme.themeChanged = function (theme) {
        var bodyElement = document.getElementsByTagName("body")[0];
        switch (theme) {
            case "dark":
            case "contrast":
                bodyElement.className = "theme-" + theme;
                break;
            case "default":
                bodyElement.className = "";
        }
    };
    return TeamsTheme;
}());

