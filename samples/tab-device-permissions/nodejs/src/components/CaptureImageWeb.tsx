// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Button, Image, Card } from '@fluentui/react-components'
import CheckAndAlertForCameraPermission from '../NavigatorPermission';
import { CardBody } from 'reactstrap';
/**
 * The 'CaptureImageWeb' component
 * of your app.
 */
const CaptureImageWeb = () => {
    var imageCapture: ImageCapture;
    const [capturedImage, setCapturedImage] = useState('');

    useEffect(() => {
        // initializing microsoft teams sdk
        microsoftTeams.app.initialize()
    }) 
    // Method to validate before capturing media
    function captureMedia() {
        // Method to ask for image capture permission and then select media
        CheckAndAlertForCameraPermission();
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
              <Text weight='bold' as="h1">Capture Image</Text>
                <CardBody>
                    <div className='flex columngap'>
                    <Text>Checks for permission before capturing image.</Text>

                        <Text weight='medium'>SDK used: </Text>
                        <Text>navigator, microsoftTeams </Text>
                        <Text weight='medium'>Method: </Text>
                        <Text>navigator.mediaDevices.getUserMedia, teams.getmedia</Text>                   
                        <Button onClick={captureMedia}>Capture image</Button>
                        <Image
                            
                            src={capturedImage}
                        />
                    </div>
                </CardBody>
            </Card>
        </>
    );
}

export default CaptureImageWeb;