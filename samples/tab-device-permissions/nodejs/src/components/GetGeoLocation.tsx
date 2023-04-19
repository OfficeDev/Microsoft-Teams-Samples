// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Button, Card} from '@fluentui/react-components'
import { CardBody } from 'reactstrap';
/**
 * The 'GetGeoLocation' component
 * of your app.
 */
const GetGeoLocation = () => {
  const [geoLocationValue, setGeoLocationValue] = useState<microsoftTeams.location.Location>({} as microsoftTeams.location.Location);
  useEffect(() => {
    microsoftTeams.app.initialize()
  })

  // Method to get current user's geo location
  // If the value of allowChooseLocation is true, then the users can choose any location of their choice.
  // If the value of allowChooseLocation is false, then the users cannot change their current location.
  // If the value of showMap is false, the current location is fetched without displaying the map. 
  // showMap is ignored if allowChooseLocation is set to true.
  function getLocation() {
    microsoftTeams.geoLocation.getCurrentLocation().then((location) => {     
      setGeoLocationValue(location)
    }).catch((error) => {    
      console.error(error);
    });
  }

  // Method to show geo location for given latitude and longitude values.
  // Method to show geo location for given latitude and longitude values.
  function showLocation() {
    // Methos to ask for permission and then show current user location
    microsoftTeams.geoLocation.map.showLocation(geoLocationValue).catch((error) => {
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

  return (
    <>
      {/* Card for Get/Show Geo-Location */}
      <Card>        
          <Text weight="bold">Get Location</Text>       
        <CardBody>
        <div className='flex columngap'>
            <Text weight="semibold">SDK used:</Text>
            <Text>navigator, microsoftTeams</Text>
            <Text weight="semibold">Method</Text>
            <Text>navigator.geolocation.getCurrentPosition, teams.location</Text>
            <Button onClick={getLocation} >Get Location</Button>
            {JSON.stringify(geoLocationValue) !== '{}' &&
              <Text>{geoLocationValue}</Text>}               
            <Button  onClick={showLocation}>Show Location</Button>
        </div>
        </CardBody>
      </Card>
    </>
  );
}

export default GetGeoLocation;