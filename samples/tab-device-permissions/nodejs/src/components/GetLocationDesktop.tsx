// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody, Image, Layout, Carousel } from '@fluentui/react-northstar'

/**
 * The 'GetLocationDesktop' component
 * of your app.
 */
const GetLocationDesktop = () => {
    const [geoLocationValue, setGeoLocationValue] = useState('');
    const [showComments, setShowComments] = useState(false);

    useEffect(() => {
        // initializing microsoft teams sdk
        microsoftTeams.app.initialize()
    })

    // Method to validate before capturing media
    function getCurrentLocation() {
        debugger;
        navigator.permissions.query({ name: 'geolocation' }).then(function (result) {
            if (result.state == 'denied') {
                setShowComments(true);
            }
            else {
                setShowComments(false);
            }
        });
        // Method to ask for image capture permission and then select media
        navigator.geolocation.getCurrentPosition(function (position) {
            var coordinates = position.coords;
            var location = {
                latitude: coordinates.latitude,
                longitude: coordinates.longitude,
            }
            setGeoLocationValue(JSON.stringify(location))
        });
    }

    return (
        <>
            {/* Card for capturing single image */}
            <Flex gap="gap.large">
                <Card>
                    <CardHeader>
                        <Text content="Get Location" weight="bold" />
                    </CardHeader>
                    <CardBody>
                        <Flex column gap="gap.small">
                            <Text content="You need to enable these permissions using App permissions icon at the top for these permissions to take effect" />
                            <Text content="After you change the app's device permissions, you will be prompted to reload the application in Teams." />
                            <Text content="SDK used: " weight="semibold" />
                            <Text content="navigator, microsoftTeams" />
                            <Text content="Method: " weight="semibold" />
                            <Text content="navigator.geolocation.getCurrentPosition, teams.location" />
                            <Button content="Get Location" onClick={() => getCurrentLocation()} disabled={showComments} />
                        </Flex>
                    </CardBody>
                    {geoLocationValue !== '' &&
                        <Text styles={{ "word-wrap": "break-word" }} content={geoLocationValue}></Text>}
                </Card>
                {showComments &&
                    <Card>
                        <CardHeader>
                            <Text content="Please note:" weight="bold" />
                        </CardHeader>
                        <CardBody>
                            <Flex column gap="gap.small">
                                <Text error content="For us to fetch your location, we will need to enable location permission in Teams." />
                                <Text error content="For Personal apps and task module dialogs, the App permissions option is available in the upper-right corner of the page." />
                                <Text error content="For Chats, channel, or meeting tabs, the App permissions option is available in the dropdown of the tab." />
                            </Flex>
                        </CardBody>
                    </Card>
                }
            </Flex>
        </>
    );
}

export default GetLocationDesktop;