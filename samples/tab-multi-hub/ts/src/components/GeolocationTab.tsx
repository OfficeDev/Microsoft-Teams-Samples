// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { app, geoLocation } from "@microsoft/teams-js";

function GeolocationTab() {
  const [isLocationVisible, setIsLocationVisible] = useState(false);
  const [platformIsSupported, setplatformIsSupported] = useState(false);
  const [currentLocation, setCurrentLocation] = useState <geoLocation.Location>();
  let hasConsent : Boolean = false
  useEffect(() => {
    app.initialize()
  })

  const getGeolocation = async () => {
    try
    {
      if (geoLocation.isSupported()) {
        const hasPerms = await geoLocation.hasPermission();

        if(!hasPerms)
        {
           hasConsent =  await geoLocation.requestPermission();
        }
        else{
          hasConsent = true
        }
        
        if(hasConsent)
        {
          const location = await geoLocation.getCurrentLocation();
          setCurrentLocation(location);
          setIsLocationVisible(true);
        }
      }
      else 
      {
        setplatformIsSupported(true);
      }
    } catch (e) {
      console.log(`GeoLocation error: ${e}`);
    }
  }

  return (
    <div className="moduleDiv">
      <h3>Get Your Geo Location:</h3>
      <br />
      <text>Please click the below button to get the current location</text>
      <br /><br />
      <button className="btnSubmit btnprofile" onClick={getGeolocation}>Click!!</button><br />
      {isLocationVisible ?
        <div style={{marginTop:10 }}>
        <table id="calendar">
          <tr>
            <td>Accuracy</td>
            <td>{currentLocation?.accuracy}</td>
          </tr>
          <tr>
            <td>Longitude</td>
            <td>{currentLocation?.longitude}</td>
          </tr>
          <tr>
            <td>Latitude</td>
            <td>{currentLocation?.latitude}</td>
          </tr>
        </table>
        </div>
      : <div></div>}
      {platformIsSupported ? <span style={{ color: 'red',marginLeft:2 }}>Sorry, This app is currently not supported on this platform.</span> : ""}
    </div>
  );
}
export default GeolocationTab;