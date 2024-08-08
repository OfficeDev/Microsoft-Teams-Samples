import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";

const Configuration = () => {
    const [tabId, setTabId] = React.useState('');

    React.useEffect(() => {
        microsoftTeams.app.initialize().then(() => {
            
        microsoftTeams.app.getContext().then(async (context) => {
            setTabId(context.page.id)
        });
       
        microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
            microsoftTeams.pages.config.setConfig({
                entityId: tabId,
                contentUrl: `${window.location.origin}/details`,
                suggestedDisplayName: 'Recruiting',
            });
            saveEvent.notifySuccess();
        });
        microsoftTeams.pages.config.setValidityState(true);
    });

    }, []);

    return (
        <div className="config-container">
            Please click on save to configure this tab
        </div>
    )
}

export default (Configuration);