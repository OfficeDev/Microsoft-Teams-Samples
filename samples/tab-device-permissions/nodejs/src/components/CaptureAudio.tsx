// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody, Video } from '@fluentui/react-northstar'

/**
 * The 'CaptureAudio' component
 * of your app.
 */
const CaptureAudio = () => {
  const [audio, setAudio] = useState('');

  useEffect(() => {
    // initializing microsoft teams sdk
    microsoftTeams.initialize()
  })

  // Method to capture media
  function captureMedia() {
    microsoftTeams.media.selectMedia({ maxMediaCount: 1, mediaType: microsoftTeams.media.MediaType.Audio }, (error: microsoftTeams.SdkError, attachments: microsoftTeams.media.Media[]) => {
      // If there's any error, an alert shows the error message/code
      if (error) {
        if (error.message) {
          alert(" ErrorCode: " + error.errorCode + error.message);
        } else {
          alert(" ErrorCode: " + error.errorCode);
        }
      }

      if (attachments) {
        // taking the first attachment  
        let audioResult = attachments[0];
        // setting state for preview
        setAudio("data:" + audioResult.mimeType + ";base64," + audioResult.preview)
      }
    });
  }

  return (
    <>
      {/* Card for showing Video/Audio */}
      <Card>
        <CardHeader>
          <Text content="Media" weight="bold" />
        </CardHeader>
        <CardBody>
          <Flex column gap="gap.small">
            <Text content="Checks for permission to use media input" />
            <Text content="SDK used: " weight="semibold" />
            <Text content="navigator, microsoftTeams" />
            <Text content="Method: " weight="semibold" />
            <Text content="navigator.mediaDevices.getUserMedia, teams.getmedia" />
            <Button content="Capture audio" onClick={captureMedia} />
          </Flex>
          {audio !== '' && <Video src={audio} variables={{ height: '50px', width: '100%', }} />}
        </CardBody>
      </Card>
    </>
  );
}

export default CaptureAudio;