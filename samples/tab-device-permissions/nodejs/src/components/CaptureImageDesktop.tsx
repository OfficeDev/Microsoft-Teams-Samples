// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody, Image } from '@fluentui/react-northstar'

/**
 * The 'CaptureImageDesktop' component
 * of your app.
 */
const CaptureImageDesktop = () => {
    var imageCapture: ImageCapture;
    const [capturedImage, setCapturedImage] = useState('');
    const [capturedImages, setCapturedImages] = useState<any[]>([]);

    useEffect(() => {
        // initializing microsoft teams sdk
        microsoftTeams.initialize()
    })

    // var imageCapture: ImageCapture;

    // Method to validate before capturing media
    function captureMedia() {
        // Method to ask for image capture permission and then select media

        navigator.permissions.query({ name: 'camera' }).then(function (result) {
            if (result.state == 'denied') {
                alert("failed");
            }
            else {
                console.log("result is" + result);
                alert("success");
            }
        });

        navigator.mediaDevices.getUserMedia({ video: true })
            .then(mediaStream => {
                const track = mediaStream.getVideoTracks()[0];
                imageCapture = new ImageCapture(track);
                imageCapture.takePhoto()
                    .then(blob => {
                        setCapturedImage(URL.createObjectURL(blob));
                    })
            })
            .catch(error => console.log(error));
    }

    return (
        <>
            {/* Card for capturing single image */}
            <Card>
                <CardHeader>
                    <Text content="Capture Image" weight="bold" />
                </CardHeader>
                <CardBody>
                    <Flex column gap="gap.small">
                        <Text content="Checks for permission before capturing image." />
                        <Text content="SDK used: " weight="semibold"/>
                        <Text content="navigator, microsoftTeams" />
                        <Text content="Method: " weight="semibold"/>
                        <Text content="navigator.mediaDevices.getUserMedia, teams.getmedia" />
                        <Button content="Capture image" onClick={captureMedia} />
                        <Image
                            fluid
                            src={capturedImage}
                        />
                    </Flex>
                </CardBody>
            </Card>
        </>
    );
}

export default CaptureImageDesktop;