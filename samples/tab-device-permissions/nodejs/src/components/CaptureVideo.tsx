// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Button, Card, CardHeader} from '@fluentui/react-components'
import { CardBody } from 'reactstrap';

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
          <Text weight="bold">Media</Text>
        </CardHeader>
        <CardBody>
          <div className='flex columngap'>
            <Text>Checks for permission to use media input</Text>
            <Text weight='medium'>SDK used: </Text>
            <Text weight='medium'>navigator, microsoftTeams</Text>
            <Text>Method</Text>
            <Text>navigator.mediaDevices.getUserMedia, teams.getmedia</Text>
            <Button onClick={captureVideo}>Capture video</Button>
          </div>          
          <video controls src={video} height={'300px'} width={'width: 100%'}  />          
       </CardBody>
      </Card>
    </>
  );
}

export default CaptureVideo;