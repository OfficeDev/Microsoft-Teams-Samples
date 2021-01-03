$(document).ready(function () {
    microsoftTeams.settings.getSettings(function (settings) {
        document.getElementById("webhook").value = settings.webhookUrl;
        microsoftTeams.settings.setValidityState(true);
    }); 
});