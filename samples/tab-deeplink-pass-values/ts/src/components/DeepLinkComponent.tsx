import * as React from "react";
import { useState, useEffect } from "react";
import { getDeepLinkForTab, getDeepLinkForWeb } from "../helpers/DeepLink";
import { Link, PrimaryButton, Stack, TextField, Text } from "@fluentui/react";

interface DeepLinkComponentProp {
    inTeams?: boolean,
    entityId?: string,
    channelId?: string
}

export const DeepLinkComponent: React.FC<DeepLinkComponentProp> = ({
    inTeams,
    entityId,
    channelId
}) => {
    const [text, setText] = useState<string>("");
    const [isValid, setIsValid] = useState<boolean>(false);
    const [deepLink, setDeepLink] = useState<string>("");
    const deepLinkType: string = inTeams ? "tab" : "standalone";

    // Create deep link based on teams/standalone environment and display to user
    const updateDeepLink = (itemId: string) => {
        const appId = process.env.APPLICATION_ID;
        const urlBaseForWeb = `https://${process.env.PUBLIC_HOSTNAME}/deeplinkstab`;
        if (inTeams && appId && entityId && channelId) {
            setDeepLink(getDeepLinkForTab(appId, entityId, itemId, channelId));
        } else if (!inTeams) {
            setDeepLink(getDeepLinkForWeb(urlBaseForWeb, itemId));
        } else {
            throw new Error("Failed to create Deep Link.");
        }
    };

    // enable button only when user has entered text
    useEffect(() => {
        if (text.length > 0) {
            setIsValid(true);
        } else {
            setIsValid(false);
        }
    }, [text]);

    return (
        <Stack>
            <TextField
                placeholder="Enter a value here"
                value={text}
                onChange={(e, data) => {
                    if (data) {
                        setText(data);
                    }
                }}
            />
            <PrimaryButton onClick={() => updateDeepLink(text)} disabled={!isValid}>Create Deep Link</PrimaryButton>
            <Stack>
                <br/>
                <Text>Deep Link for {deepLinkType} app: </Text>
                <Link href={deepLink} target='_blank'>{deepLink}</Link>
            </Stack>
        </Stack>
    );
};
