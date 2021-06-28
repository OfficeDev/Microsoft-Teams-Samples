import * as microsoftTeams from "@microsoft/teams-js";
const Config = () => {
    const baseUrl = `https://${window.location.hostname}:${window.location.port}`;
    microsoftTeams.initialize();
    microsoftTeams.settings.registerOnSaveHandler((saveEvent) => {
        microsoftTeams.settings.setSettings({
            contentUrl: baseUrl + "/",
            entityId: "DetailsTab",
            suggestedDisplayName: "DetailsTab",
            websiteUrl: baseUrl + "/",
        });
        saveEvent.notifySuccess();
    });
microsoftTeams.settings.setValidityState(true);
return (
    <div>
        <div style={{display: "flex", FontSize: 18}}>Press save to continue</div>
    </div>
)
};
export default Config;