import React from 'react';
import { app, pages } from "@microsoft/teams-js";
// import * as microsoftTeams from "@microsoft/teams-js";

const Configuration = () => {
    const [tabId, setTabId] = React.useState('');

    React.useEffect(() => {
        app.initialize().then(() => {
            app.notifySuccess();
            app.getContext().then((context: app.Context) => {
                setTabId(context.page.id)
                pages.config.registerOnSaveHandler(function (saveEvent) {
                    pages.config.setConfig({
                        entityId: tabId,
                        contentUrl: `${window.location.origin}/details`,
                        suggestedDisplayName: 'Recruiting',
                    }).then(() => {
                        saveEvent.notifySuccess();
                    });
                });
                pages.config.setValidityState(true);
            });
        });
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
    }, []);

    return (
        <div className="config-container">
            Please click on Save to configure this tab
        </div>
    )
}

export default (Configuration);