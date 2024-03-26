import * as React from "react";
import { Header } from "@fluentui/react-northstar";
import { useState, useEffect } from "react";
import { useTeams } from "msteams-react-base-component";
import { app } from "@microsoft/teams-js";
import { getPassedValue } from "../helpers/URLPath";
import { DeepLinkComponent } from "../components/DeepLinkComponent";
import { Link, Stack, Text } from "@fluentui/react";

/**
 * Implementation of the DeepLinks Tab content page
 */
export const DeepLinksTab = () => {

    const [{ inTeams, theme, context }] = useTeams();
    // will remain undefined if not in teams environment
    const [entityId, setEntityId] = useState<string | undefined>();
    // will remain undefined if not in teams environment
    const [channelId, setChannelId] = useState<string | undefined>();
    // will remain undefined if app was not accessed using a deep link
    const [passedValue, setPassedValue] = useState<string | undefined>(getPassedValue());

    useEffect(() => {
        if (inTeams === true) {
            app.notifySuccess();
        }
    }, [inTeams]);

    useEffect(() => {
        if (context) {
            setEntityId(context.page.id);
            setChannelId(context.channel?.id);
            setPassedValue(context.page.subPageId);
        }
    }, [context]);

    /**
     * The render() method to create the UI of the tab
     */
    return (
        <Stack>
            <Stack>
                <Header content="Sample App - Using Deep Links to Pass Values" />
                <Stack>
                    <Header as="h2" content="Background" />
                    <Text>
                        Deep links can be used to pass a value to an application when it loads.
                        The application can then use that value to perform operations like internal page navigation, DB searches, and more.
                        Using deep links, you can quickly lead users to specific content in your application.
                    </Text>
                    <Text>
                        In Microsoft Teams, deep links are expected to follow a specific format.
                        If you want your app to support deep links for both Microsoft Teams and direct URL access, then
                        you will need to use two different URL formatting approaches.
                    </Text>
                    <Text>
                        For more information, please see:{" "}
                        <Link href="https://learn.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/deep-link-application?tabs=teamsjs-v2" target='_blank'>
                            MS Teams Deep Link Documentation
                        </Link>
                    </Text>
                    <Header as="h2" content="Instructions" />
                    <Text>
                        Under &quot;Generate a deep link&quot;, enter a value in the text box and click the button below to generate a deep link for this app.
                        When the user clicks the link, it will open the app and pass on the entered value.
                        The passed value will be displayed under &quot;Current Access Information&quot;.
                    </Text>
                    <Text>
                        If you are accessing the app through Teams (&quot;tab app&quot;), then the deep link will open in Teams.
                        If you are accessing the app directly through its public URL (&quot;standalone app&quot;), then the deep link will open in the standalone version.
                        To open the standalone app from the tab app, right-click the tab and select &quot;Open in browser&quot; or &quot;Open in new window&quot;.
                    </Text>
                </Stack>
                <Stack>
                    <Header as="h2" content="Current Access Information" />
                    { passedValue
                        ? <Text>This app was accessed using a deep link. It passed along this value: <strong>{passedValue}</strong></Text>
                        : <Text>This app was NOT accessed using a deep link.</Text>
                    }
                </Stack>
                <Stack>
                    <Header as="h2" content="Generate a deep link" />
                    <DeepLinkComponent
                        inTeams={inTeams}
                        entityId={entityId}
                        channelId={channelId}
                    />
                </Stack>
            </Stack>
        </Stack>
    );
};
