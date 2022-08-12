import * as microsoftTeams from "@microsoft/teams-js";
const Config = () => {
    const baseUrl = `https://${window.location.hostname}:${window.location.port}`;
    microsoftTeams.app.initialize().then(() => {
        microsoftTeams.pages.config.registerOnSaveHandler((saveEvent) => {
            microsoftTeams.pages.config.setConfig({
                contentUrl: baseUrl + "/",
                entityId: "DetailsTab",
                suggestedDisplayName: "DetailsTab",
                websiteUrl: baseUrl + "/",
            });
            saveEvent.notifySuccess();
        });
        microsoftTeams.pages.config.setValidityState(true);
    });
    
return (
    <div>
        <div style={{display: "flex", FontSize: 18}}>Press save to continue</div>
    </div>
)
};
export default Config;