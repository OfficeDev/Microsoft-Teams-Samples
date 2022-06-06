// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody, Image } from '@fluentui/react-northstar'

/**
 * The 'GetNotificationDesktop' component
 * of your app.
 */
const GetNotificationDesktop = () => {

    useEffect(() => {
        // initializing microsoft teams sdk
        microsoftTeams.app.initialize()
    })

    // Method to validate and send notification
    function sendNotification() {
        // Method to ask for validating notification permission and then sending notification.

        navigator.permissions.query({ name: 'notifications' }).then(function (result) {
            if (result.state == 'denied') {
                alert("failed");
            }
            else {
                console.log("result is" + result);
                alert("success");
            }

            if (Notification.permission !== 'granted')
                Notification.requestPermission();
            else {
                var notification = new Notification('Sample Notification', {
                    body: 'Hey there! You\'ve been notified!',
                });
                notification.onclick = function () {
                    window.open('https://github.com/OfficeDev/Microsoft-Teams-Samples');
                };
            }
        });
    }

    return (
        <>
            {/* Card for sending notification */}
            <Card>
                <CardHeader>
                    <Text content="Notifications (Web only)" weight="bold" />
                </CardHeader>
                <CardBody>
                    <Flex column gap="gap.small">
                        <Text content="Checks for permission before getting notification." />
                        <Text content="SDK used: " weight="semibold" />
                        <Text content="navigator" />
                        <Text content="Method: " weight="semibold" />
                        <Text content="Notifications API" />
                        <Button content="Notify" onClick={sendNotification} />
                    </Flex>
                </CardBody>
            </Card>
        </>
    );
}

export default GetNotificationDesktop;