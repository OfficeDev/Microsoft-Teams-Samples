import * as React from "react";
import { useState, useEffect, useRef } from "react";
import { useTeams } from "msteams-react-base-component";
import { app, pages } from "@microsoft/teams-js";
import { Stack, Text } from "@fluentui/react";

/**
 * Implementation of Deep Links Tab configuration page
 */
export const DeepLinksTabConfig = () => {

    const [{ inTeams, theme, context }] = useTeams({});
    const [text, setText] = useState<string>();
    const entityId = useRef("MyEntityId");

    const onSaveHandler = (saveEvent: pages.config.SaveEvent) => {
        const host = "https://" + window.location.host;
        pages.config.setConfig({
            contentUrl: host + "/deepLinksTab/",
            websiteUrl: host + "/deepLinksTab/",
            suggestedDisplayName: "Deep Links Tab",
            removeUrl: host + "/deepLinksTab/remove.html?theme={theme}",
            entityId: entityId.current
        }).then(() => {
            saveEvent.notifySuccess();
        });
    };

    useEffect(() => {
        if (context) {
            setText(context.page.id);
            pages.config.registerOnSaveHandler(onSaveHandler);
            pages.config.setValidityState(true);
            app.notifySuccess();
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [context]);

    return (
        <Stack>
            <Text>Configure your tab</Text>
        </Stack>
    );
};
