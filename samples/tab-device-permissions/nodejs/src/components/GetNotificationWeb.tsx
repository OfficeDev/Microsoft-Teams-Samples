// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import {Text, Button, Card } from '@fluentui/react-components'
import { CardBody } from 'reactstrap';
/**
 * The 'GetNotificationWeb' component
 * of your app.
 */
const GetNotificationWeb = () => {

    useEffect(() => {
        // initializing microsoft teams sdk
        microsoftTeams.app.initialize()
    })

    // Method to validate and send notification
    function sendNotification() {
        // Method to ask for validating notification permission and then sending notification.        
        navigator.permissions.query({ name: 'notifications' }).then(function (result) {
            if (result.state === 'denied') {
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
            <Text weight='bold' as="h1">Notifications</Text>                
                <CardBody>
                    <div className='flex columngap'>
                        <Text>Checks for permission before getting notification.</Text>
                        <Text weight="semibold">SDK used: </Text>
                        <Text>Navigator</Text>
                        <Text weight="semibold">Method: </Text>
                        <Text>Notifications API</Text>
                        <Button onClick={sendNotification}>Notify</Button>
                    </div>
                </CardBody>
            </Card>
        </>
    );
}

export default GetNotificationWeb;