// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Button, Card, CardHeader} from '@fluentui/react-components'
import { CardBody } from 'reactstrap';

/**
 * The 'GetLocationWeb' component
 * of your app.
 */
const GetLocationWeb = () => {
    const [geoLocationValue, setGeoLocationValue] = useState('');
    const [showComments, setShowComments] = useState(false);

    useEffect(() => {
        // initializing microsoft teams sdk
        microsoftTeams.app.initialize()
    })

    // Method to validate before capturing media
    function getCurrentLocation() {        
        navigator.permissions.query({ name: 'geolocation' }).then(function (result) {
            if (result.state === 'denied') {
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
            <Card>
                <div>
                <Text weight='bold' as="h1">Get Location</Text>
                  
                    <CardBody>
                        <div className='flex columngap'>
                            <Text>You need to enable these permissions using App permissions icon at the top for these permissions to take effect</Text>
                            <Text>After you change the app's device permissions, you will be prompted to reload the application in Teams.</Text>
                            <Text weight="semibold">SDK used:</Text>
                            <Text> navigator, microsoftTeams</Text>
                            <Text weight="semibold">Method</Text>
                            <Text>navigator.geolocation.getCurrentPosition, teams.location</Text>
                            <Button onClick={() => getCurrentLocation()} disabled={showComments}>Get Location</Button>
                        </div>
                    </CardBody>
                    {geoLocationValue !== '' &&
                        <Text> {geoLocationValue}</Text>}
                </div>
                {showComments &&
                    <div>
                        <CardHeader>
                            <Text weight="bold">Please Note</Text>
                        </CardHeader>
                        <CardBody>
                            <div>
                                <Text>For us to fetch your location, we will need to enable location permission in Teams.</Text>
                                <Text>For Personal apps and task module dialogs, the App permissions option is available in the upper-right corner of the page.</Text>
                                <Text>For Chats, channel, or meeting tabs, the App permissions option is available in the dropdown of the tab.</Text>
                            </div>
                        </CardBody>
                    </div>
                }
            </Card>
        </>
    );
}

export default GetLocationWeb;