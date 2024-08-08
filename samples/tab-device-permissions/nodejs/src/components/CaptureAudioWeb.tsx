// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Button, Card } from '@fluentui/react-components'
import { CardBody } from 'reactstrap';
/**
 * The 'CaptureAudioWeb' component
 * of your app.
 */
const CaptureAudioWeb = () => {
    useEffect(() => {
        // initializing microsoft teams sdk
        microsoftTeams.app.initialize()
    })
    function captureAudio() {
        navigator.mediaDevices.getUserMedia({ audio: true })
            .then(mediaStream => {
                const audioElement = document.querySelector("audio");
                audioElement!.srcObject = mediaStream;
            })
            .catch(error => console.log(error));
    }
    return (
        <>
            {/* Card for showing Audio */}
            <Card>
            <Text weight='bold' as="h1">Capture Audio </Text>              
                <CardBody>
                    <div className='flex columngap'>
                    <Text>Checks for permission to use media input</Text>
                    <Text weight='medium'>SDK used</Text>
                    <Text>navigator, microsoftTeams</Text>
                    <Text weight='medium'>Method:</Text>
                    <Text>navigator.mediaDevices.getUserMedia, teams.getmedia</Text>
                    <Button onClick={captureAudio}>Capture audio</Button> 
                        <audio controls></audio>
                    </div>
                </CardBody>
            </Card>
        </>
    );
}

export default CaptureAudioWeb;