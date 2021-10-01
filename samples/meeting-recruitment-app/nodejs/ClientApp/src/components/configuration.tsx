import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";

const Configuration = () => {
    const [tabId, setTabId] = React.useState('');

    React.useEffect(() => {
        microsoftTeams.initialize();

        microsoftTeams.getContext(async (context: microsoftTeams.Context) => {
            setTabId(context.entityId)
        });

        microsoftTeams.settings.registerOnSaveHandler(async (saveEvent: microsoftTeams.settings.SaveEvent) => {
            microsoftTeams.settings.setSettings({
                entityId: tabId,
                contentUrl: `${window.location.origin}/details`,
                suggestedDisplayName: 'Recruiting',
            });
            saveEvent.notifySuccess();
        });
        microsoftTeams.settings.setValidityState(true);
    }, []);

    return (
        <div className="config-container">
            Please click on save to configure this tab
        </div>
    )
}

export default (Configuration);