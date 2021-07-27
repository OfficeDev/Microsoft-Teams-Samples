// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody } from '@fluentui/react-northstar'

/**
 * The 'GetGeoLocation' component
 * of your app.
 */
const GetGeoLocation = () => {
 const [geoLocationValue, setGeoLocationValue] = useState('');
 useEffect(() => {
  microsoftTeams.initialize()
 })
 
 // Method to get current user's geo location
 // If the value of allowChooseLocation is true, then the users can choose any location of their choice.
 // If the value of allowChooseLocation is false, then the users cannot change their current location.
 // If the value of showMap is false, the current location is fetched without displaying the map. 
 // showMap is ignored if allowChooseLocation is set to true.
  function getLocation () {
    microsoftTeams.location.getLocation({ allowChooseLocation: true, showMap: true }, (error: microsoftTeams.SdkError, location: microsoftTeams.location.Location) => {
        setGeoLocationValue(JSON.stringify(location))
    });
  }

  // Method to show geo location for given latitude and longitude values.
  function showLocation () {
  // The method will show location based on latitude and longitute values provided.    
  let location = {"latitude":28.704059,"longitude":77.10249};
  // Methos to ask for permission and then show current user location
  microsoftTeams.location.showLocation(location, (error: microsoftTeams.SdkError, result: boolean) => {
        // If there's any error, an alert shows the error message/code
        if (error) {
            if (error.message) {
              alert(" ErrorCode: " + error.errorCode + error.message);
            } else {
              alert(" ErrorCode: " + error.errorCode);
            }
             return;
        }
    });
  }

  return(
     <>
      {/* Card for Get/Show Geo-Location */}
      <Card>
        <CardHeader>
          <Text content="Get Geo Location (Mobile Only)" weight="bold" />
        </CardHeader>
        <CardBody>
        <Flex column gap="gap.small">
          <Button content="Get Location" onClick={getLocation}/>
         {geoLocationValue !== '' && 
         <Text styles={{"word-wrap":"break-word"}} content={geoLocationValue}></Text>}
          <Button content="Show Location" onClick={showLocation}/>
        </Flex>
        </CardBody>
      </Card>
      </>
    );
}

export default GetGeoLocation;