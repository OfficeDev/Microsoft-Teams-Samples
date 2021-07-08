// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React, { useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody } from '@fluentui/react-northstar'

/**
 * The 'PersonalTab' component renders the main tab content
 * of your app.
 */
const Tab = () => {
  //React lifecycle method that gets called once a component has finished mounting
  //Learn more: https://reactjs.org/docs/react-component.html#componentdidmount
 useEffect(() => {
  microsoftTeams.initialize()
  // Get the user context from Teams and set it in the state
  microsoftTeams.getContext((context: microsoftTeams.Context) => {
  });
 })

 function scanBarCode()
 {
  const config: microsoftTeams.media.BarCodeConfig = {
    timeOutIntervalInSec: 30};
  microsoftTeams.media.scanBarCode((error: microsoftTeams.SdkError, decodedText: string) => {
    if (error) {
      if (error.message) {
        alert(" ErrorCode: " + error.errorCode + error.message);
      } else {
        alert(" ErrorCode: " + error.errorCode);
      }
    } else if (decodedText) {
      alert(decodedText);
    }
  }, config);
 }

 function captureImage(){
  microsoftTeams.media.captureImage((error: microsoftTeams.SdkError, files: microsoftTeams.media.File[]) => {
   
  });
 }

   //method to validate before capturing media.
  function captureMultipleImages() {
    microsoftTeams.media.selectMedia({ maxMediaCount: 10, mediaType: microsoftTeams.media.MediaType.Image }, (error: microsoftTeams.SdkError, attachments: microsoftTeams.media.Media[]) => {
      /* ... */
    });
  }

  function captureAudio() {
    microsoftTeams.media.selectMedia({ maxMediaCount: 1, mediaType: microsoftTeams.media.MediaType.Audio }, (error: microsoftTeams.SdkError, attachments: microsoftTeams.media.Media[]) => {
      /* ... */
    });
  }
  
  function getLocation () {
      microsoftTeams.location.getLocation({ allowChooseLocation: true, showMap: true }, (error: microsoftTeams.SdkError, location: microsoftTeams.location.Location) => {
        alert(JSON.stringify(location))
       });
  }

  // Method to show geo location for given latitude and longitude values.
  function showLocation () {
  let location = {"latitude":28.704059,"longitude":77.10249};
  microsoftTeams.location.showLocation(location, (err: microsoftTeams.SdkError, result: boolean) => {
          if (err) {
            alert(err);
            return;
          }
    });
  }

  function selectPeople()
 {
  microsoftTeams.people.selectPeople((error: microsoftTeams.SdkError, people: microsoftTeams.people.PeoplePickerResult[]) => 
 {
    if (error) 
    {
        if (error.message) 
           {
             alert(" ErrorCode: " + error.errorCode + error.message);
           }
            else 
            {
              alert(" ErrorCode: " + error.errorCode);
            }
      }
    if (people)
     {
            alert("People length: " + people.length);
      }
  });
 }
 
    return(
      <Flex column gap="gap.large">
       {/* Card for capturing single image */}
      <Card>
        <CardHeader>
          <Text content="Capture Image (Mobile Only)" weight="bold" />
        </CardHeader>
        <CardBody>
        <Flex column gap="gap.small">
        <Text content="Checks for permission before capturing image" />
          <Button content="Capture Image" onClick={captureImage}/>
        </Flex>
        </CardBody>
      </Card>

      {/* Card for showing multiple images */}
      <Card>
        <CardHeader>
          <Text content="Capture Multiple Image (Mobile Only)" weight="bold" />
        </CardHeader>
        <CardBody>
        <Flex column gap="gap.small">
        <Text content="Checks for permission before capturing image. You can capture multiple images or select them grom gallery." />
          <Button content="Capture multiple images" onClick={captureMultipleImages}/>
        </Flex>
        </CardBody>
      </Card>

       {/* Card for showing Video/Audio */}
       <Card>
        <CardHeader>
          <Text content="Media (Mobile Only)" weight="bold" />
        </CardHeader>
        <CardBody>
        <Flex column gap="gap.small">
        <Text content="Checks for permission to use media input" />
          <Button content="Capture audio" onClick={captureAudio}/>
        </Flex>
        </CardBody>
      </Card>

      {/* Card for Barcode Scanner */}
      <Card>
        <CardHeader>
          <Text content="Scan Barcode (Mobile Only)" weight="bold" />
        </CardHeader>
        <CardBody>
        <Flex column gap="gap.small">
        <Text content="Scan any barcode to get information related to it" />
           <Button content="Scan Barcode" onClick={scanBarCode}/>
        </Flex>
        </CardBody>
      </Card>

      {/* Card for People Picker */}
      <Card>
        <CardHeader>
          <Text content="People Picker (Mobile Only)" weight="bold" />
        </CardHeader>
        <CardBody>
        <Flex column gap="gap.small">
          <Button content="People Picker" onClick={selectPeople}/>
        </Flex>
        </CardBody>
      </Card>

      {/* Card for Get/Show Geo-Location */}
      <Card>
        <CardHeader>
          <Text content="Get Geo Location (Mobile Only)" weight="bold" />
        </CardHeader>
        <CardBody>
        <Flex column gap="gap.small">
          <Button content="Get Location" onClick={getLocation}/>
          <Button content="Show Location" onClick={showLocation}/>
        </Flex>
        </CardBody>
      </Card>
      </Flex>
    );
}
export default Tab;