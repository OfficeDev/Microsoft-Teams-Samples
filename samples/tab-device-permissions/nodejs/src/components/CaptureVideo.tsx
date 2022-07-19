// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody, Video } from '@fluentui/react-northstar'

// The 'CaptureVideo' component of your app.
const CaptureVideo = () => {
  const [video, setVideo] = useState('');

  useEffect(() => {
    // initializing microsoft teams sdk
    microsoftTeams.app.initialize()
  })

  let videoControllerCallback: microsoftTeams.media.VideoControllerCallback = {
    onRecordingStarted() {
      alert('onRecordingStarted Callback Invoked');
    },
  };

  
  const defaultNativeVideoProps: microsoftTeams.media.VideoProps = {
    maxDuration: 30, // the maximumDuration is the time in seconds after which the recording should terminate automatically. This value can be changed.
    isFullScreenMode: true,
    isStopButtonVisible: false,
    videoController: new microsoftTeams.media.VideoController(videoControllerCallback)
  }

  const defaultNativeVideoMediaInput: microsoftTeams.media.MediaInputs = {
    mediaType: microsoftTeams.media.MediaType.Video,
    maxMediaCount: 1,
    videoProps: defaultNativeVideoProps
  }

  function captureVideo() {
    microsoftTeams.media.selectMedia(defaultNativeVideoMediaInput, (error: microsoftTeams.SdkError, attachments: microsoftTeams.media.Media[]) => {
      // If there's any error, an alert shows the error message/code
      if (error) {
        if (error.message) {
          alert(" ErrorCode: " + error.errorCode + error.message);
        } 
        else {
          alert(" ErrorCode: " + error.errorCode);
        }
      }

      if (attachments) {
        // setting state for preview
        let media: microsoftTeams.media.Media = attachments[0]
        media.getMedia((error: microsoftTeams.SdkError, blob: Blob) => {
        if (blob) {
            if (blob.type.includes("video")) {
                setVideo(URL.createObjectURL(blob));
            }
        }
        if (error) {
            if (error.message) {
                alert(" ErrorCode: " + error.errorCode + error.message);
            } 
            else {
                alert(" ErrorCode: " + error.errorCode);
            }
        }
});
      }
    });
  }

  return (
    <>
      {/* Card for showing Video */}
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
            <Button content="Capture video" onClick={captureVideo} />
          </Flex>
          {video !== '' && <Video src={video} variables={{ height: '300px', width: '100%', }} />}
        </CardBody>
      </Card>
    </>
  );
}

export default CaptureVideo;