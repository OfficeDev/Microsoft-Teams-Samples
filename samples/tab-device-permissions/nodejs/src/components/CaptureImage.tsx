// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Button, Image, Card, CardHeader} from '@fluentui/react-components'
import { CardBody} from 'reactstrap';
import { Carousel } from 'rsuite';

/**
 * The 'CaptureImage' component
 * of your app.
 */

const CaptureImage = () => {
  const [capturedImage] = useState('');
  const [capturedImages, setCapturedImages] = useState<any[]>([]);

  useEffect(() => {
    // initializing microsoft teams sdk
    microsoftTeams.app.initialize()
  })

  // Method to validate before capturing media
  function captureMultipleImages(mediaCount: number) {
    // Method to ask for image capture permission and then select media
       let imageProp: microsoftTeams.media.ImageProps = {
      sources: [microsoftTeams.media.Source.Camera, microsoftTeams.media.Source.Gallery],
      startMode: microsoftTeams.media.CameraStartMode.Photo,
      ink: false,
      cameraSwitcher: false,
      textSticker: false,
      enableFilter: true,
    };

   
    let mediaInput: microsoftTeams.media.MediaInputs = {
      mediaType: microsoftTeams.media.MediaType.Image,
      maxMediaCount: mediaCount,
      imageProps: imageProp
    };
    
    microsoftTeams.media.selectMedia(mediaInput, (error: microsoftTeams.SdkError, attachments: microsoftTeams.media.Media[]) => {
      // If there's any error, an alert shows the error message/code
      if (error) {
        if (error.message) {
          alert(" ErrorCode: " + error.errorCode + error.message);
        } else {
          alert(" ErrorCode: " + error.errorCode);
        }
      } else if (attachments) {

        // creating selected images array to show preview 
        const imageArray: any[] = attachments.map((item, index) => {         
          return (
                <img alt='img'
                  src={"data:" + item.mimeType + ";base64," + item.preview}  
                />
              )
        })
        setCapturedImages(imageArray);
      }
    });
  }

  return (
    <>
      {/* Card for capturing single image */}
      <Card>
        
          <Text weight="bold">Capture Image</Text>
        
        <CardBody>
          <div className='columngap'>
            <Text>Checks for permission before capturing image.</Text>
            <Button onClick={() => captureMultipleImages(1)} >Capture Image</Button>
          </div>
        </CardBody>
        {capturedImage !== '' &&
          <div className="wrapper">
          
          <div className="box2"> renderMainArea={() => (
              <Image                
                src={"data:image/png;base64," + capturedImage}
              />
            )}</div>          
        </div>
        }
      </Card>
      {/* Card for showing multiple images */}
      <Card>
        <CardHeader>
          <Text weight="bold">Capture Multiple Image (Mobile Only)</Text>
        </CardHeader>
        <CardBody>
          <div>
            <Button onClick={() => captureMultipleImages(2)}>Capture multiple images</Button>
          </div>           
          {capturedImages.length !== 0 &&             
            <Carousel className="custom-slider">             
            {capturedImages}
          </Carousel>
          }
        </CardBody>
      </Card>
    </>
  );
}

export default CaptureImage;