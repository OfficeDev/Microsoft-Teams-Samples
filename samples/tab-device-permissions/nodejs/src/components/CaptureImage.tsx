// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody, Image, Layout, Carousel } from '@fluentui/react-northstar'

/**
 * The 'CaptureImage' component
 * of your app.
 */
const CaptureImage = () => {
  const [capturedImage, setCapturedImage] = useState('');
  const [capturedImages, setCapturedImages] = useState<any[]>([]);

  useEffect(() => {
    // initializing microsoft teams sdk
    microsoftTeams.initialize()
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
            {
              key: index,
              id: index,
              content: (
                <Image
                  src={"data:" + item.mimeType + ";base64," + item.preview}
                  fluid
                  alt={'image'}
                />
              ),
              'aria-label': "images",
            }
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
        <CardHeader>
          <Text content="Capture Image" weight="bold" />
        </CardHeader>
        <CardBody>
          <Flex column gap="gap.small">
            <Text content="Checks for permission before capturing image." />
            <Button content="Capture Image" onClick={() => captureMultipleImages(1)} />
          </Flex>
        </CardBody>
        {capturedImage !== '' &&
          <Layout styles={{ maxWidth: '150px', }}
            renderMainArea={() => (
              <Image
                fluid
                src={"data:image/png;base64," + capturedImage}
              />
            )} />}
      </Card>

      {/* Card for showing multiple images */}
      <Card>
        <CardHeader>
          <Text content="Capture Multiple Image (Mobile Only)" weight="bold" />
        </CardHeader>
        <CardBody>
          <Flex column gap="gap.small">
            <Button content="Capture multiple images" onClick={() => captureMultipleImages(2)} />
          </Flex>
          {capturedImages.length !== 0 &&
            <Carousel
              ariaRoleDescription="carousel"
              ariaLabel="Selected images"
              navigation={{
                'aria-label': 'selected images',
                items: capturedImages.map((item, index) => ({
                  key: index,
                  'aria-controls': item.id,
                  content: item.thumbnail,
                })),
              }}
              items={capturedImages}
              getItemPositionText={(index, size) => `${index + 1} of ${size}`}
            />
          }
        </CardBody>
      </Card>
    </>
  );
}

export default CaptureImage;