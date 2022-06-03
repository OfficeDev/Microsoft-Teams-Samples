$(document).ready(function () {
    microsoftTeams.pages.getConfig().then((settings) => {
        document.getElementById("webhook").value = settings.webhookUrl;
        microsoftTeams.pages.config.setValidityState(true);
    }); 
});