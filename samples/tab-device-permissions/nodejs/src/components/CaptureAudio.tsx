// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import {  Text, Button, Card} from '@fluentui/react-components'
import { CardBody } from 'reactstrap';
/**
 * The 'CaptureAudio' component
 * of your app.
 */
const CaptureAudio = () => {
  const [audio, setAudio] = useState('');

  useEffect(() => {
    // initializing microsoft teams sdk
    microsoftTeams.app.initialize()
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
      <Text weight='bold' as="h1">Media</Text>  
       <CardBody>          
          <div className='flex columngap'>
            <Text>Checks for permission to use media input</Text>
            <Text weight='medium'>SDK used</Text>
            <Text>navigator, microsoftTeams</Text>
            <Text weight='medium'>Method:</Text>
            <Text>navigator.mediaDevices.getUserMedia, teams.getmedia</Text>                   
              <Button onClick={captureMedia}>Capture audio</Button>
          </div> 
          <audio controls src={audio} />    
      </CardBody>
      </Card>
    </>
  );
}

export default CaptureAudio;