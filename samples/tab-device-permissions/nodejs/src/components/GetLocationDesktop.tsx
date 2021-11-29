// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody, Image, Layout, Carousel } from '@fluentui/react-northstar'
import image from "../permissionIcon.png";

/**
 * The 'GetLocationDesktop' component
 * of your app.
 */
const GetLocationDesktop = () => {
    const [geoLocationValue, setGeoLocationValue] = useState('');

    useEffect(() => {
        // initializing microsoft teams sdk
        microsoftTeams.initialize()
    })

    // Method to validate before capturing media
    function getCurrentLocation() {
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
            <Card>
                <CardHeader>
                    <Text content="Get Location (Browser support)" weight="bold" />
                </CardHeader>
                <CardBody>
                    <Flex column gap="gap.small">
                        <Text content="You need to enable these permissions using App permissions icon at the top for these permissions to take effect" />
                        <Text content="After you change the app's device permissions, you will be prompted to reload the application in Teams." />
                        <Button content="Get Location" onClick={() => getCurrentLocation()} />
                    </Flex>
                </CardBody>
                { geoLocationValue !== '' && 
         <Text styles={{"word-wrap":"break-word"}} content={geoLocationValue}></Text> }
            </Card>
        </>
    );
}

export default GetLocationDesktop;