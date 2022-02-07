// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody } from '@fluentui/react-northstar'

/**
 * The 'CaptureAudioDesktop' component
 * of your app.
 */
const CaptureAudioDesktop = () => {
    const [capturedVideo, setCapturedVideo] = useState(new MediaStream);
    useEffect(() => {
        // initializing microsoft teams sdk
        microsoftTeams.initialize()
    })

    function captureAudio() {
        navigator.mediaDevices.getUserMedia({ audio: true })
            .then(mediaStream => {
                const audioElement = document.querySelector("audio");
                audioElement!.srcObject = mediaStream;
                setCapturedVideo(mediaStream);
            })
            .catch(error => console.log(error));
    }

    return (
        <>
            {/* Card for showing Audio */}
            <Card>
                <CardHeader>
                    <Text content="Capture Audio" weight="bold" />
                </CardHeader>
                <CardBody>
                    <Flex column gap="gap.small">
                        <Text content="Checks for permission to use media input" />
                        <Text content="SDK used: " weight="semibold" />
                        <Text content="navigator, microsoftTeams" />
                        <Text content="Method: " weight="semibold" />
                        <Text content="navigator.mediaDevices.getUserMedia, teams.getmedia" />
                        <Button content="Capture audio" onClick={captureAudio} />
                        <audio controls></audio>
                    </Flex>
                </CardBody>
            </Card>
        </>
    );
}

export default CaptureAudioDesktop;