import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";

const Configuration = () => {
    const [tabId, setTabId] = React.useState('');

    React.useEffect(() => {
        //microsoftTeams.initialize();

        //microsoftTeams.getContext(async (context: microsoftTeams.Context) => {
        //    setTabId(context.entityId)
        //});

        //microsoftTeams.settings.registerOnSaveHandler(async (saveEvent: microsoftTeams.settings.SaveEvent) => {
        //    microsoftTeams.settings.setSettings({
        //        entityId: tabId,
        //        contentUrl: `${window.location.origin}/details`,
        //        suggestedDisplayName: 'Recruiting',
        //    });
        //    saveEvent.notifySuccess();
        //});
        //microsoftTeams.settings.setValidityState(true);

        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.getContext((context: any) => {
                setTabId(context.entityId)
            })

            microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
                microsoftTeams.pages.config.setConfig({
                    entityId: tabId,
                    suggestedDisplayName: "Recruiting",
                    contentUrl: `${window.location.origin}/details`,
                });
                saveEvent.notifySuccess();
            });
            microsoftTeams.pages.config.setValidityState(true);
        });

    }, []);

    return (
        <div className="config-container">
            Please click on Save to configure this tab
        </div>
    )
}

export default (Configuration);