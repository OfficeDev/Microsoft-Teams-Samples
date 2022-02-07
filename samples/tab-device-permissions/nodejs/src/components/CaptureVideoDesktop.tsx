// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody } from '@fluentui/react-northstar'

/**
 * The 'captureVideoDesktop' component
 * of your app.
 */
const CaptureVideoDesktop = () => {
    //  var stream: MediaStream = null;
    const [capturedVideo, setCapturedVideo] = useState(new MediaStream);
    useEffect(() => {
        // initializing microsoft teams sdk
        microsoftTeams.initialize()
    })

    function captureVideo() {
        navigator.mediaDevices.getUserMedia({ video: true })
            .then(mediaStream => {
                const videoElement = document.querySelector("video");
                videoElement!.srcObject = mediaStream;
                setCapturedVideo(mediaStream);
            })
            .catch(error => console.log(error));
    }

    return (
        <>
            {/* Card for showing Video */}
            <Card>
                <CardHeader>
                    <Text content="Capture Video (Web only)" weight="bold" />
                </CardHeader>
                <CardBody>
                    <Flex column gap="gap.small">
                        <Text content="Checks for permission to use media input" />
                        <Text content="SDK used: " weight="semibold" />
                        <Text content="navigator" />
                        <Text content="Method: " weight="semibold" />
                        <Text content="navigator.mediaDevices.getUserMedia" />
                        <Button content="Capture video" onClick={captureVideo} />
                        <video controls ></video>
                    </Flex>
                </CardBody>
            </Card>
        </>
    );
}

export default CaptureVideoDesktop;