// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React, { useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";

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

  //method to validate before capturing media.
  function captureMedia() {
      navigator.permissions.query({name:'microphone'}).then(function(result) {
        console.log(result)
      if (result.state == 'granted') {
        console.log(result)
        let mediaInput: microsoftTeams.media.MediaInputs = {
          mediaType: microsoftTeams.media.MediaType.Audio,
          maxMediaCount: 1,
      };
      microsoftTeams.media.selectMedia(mediaInput, (error: microsoftTeams.SdkError, attachments: microsoftTeams.media.Media[]) => {
        debugger
          if (error) {
              if (error.message) {
                  alert(" ErrorCode: " + error.errorCode + error.message);
              } else {
                  alert(" ErrorCode: " + error.errorCode);
              }
          }
          // If you want to directly use the audio file (for smaller file sizes (~4MB))    if (attachments) {
          let audioResult = attachments[0];
          var videoElement = document.createElement("video");
          videoElement.setAttribute("src", ("data:" + audioResult.mimeType + ";base64," + audioResult.preview));
          //To use the audio file via get Media API for bigger audio file sizes greater than 4MB       
          audioResult.getMedia((error: microsoftTeams.SdkError, blob: Blob) => {
          if (blob) {
              if (blob.type.includes("video")) {
                  videoElement.setAttribute("src", URL.createObjectURL(blob));
              }
          }
          if (error) {
              if (error.message) {
                  alert(" ErrorCode: " + error.errorCode + error.message);
              } else {
                  alert(" ErrorCode: " + error.errorCode);
              }
          }
      });
      }); 
    } 
    else if (result.state == 'prompt') {
        // Access has not been granted
        console.log(result)
        navigator.mediaDevices.getUserMedia({ audio: true, video: true });
      }
    });
  }

    return(
      <>
       <div>Tab content</div>
       <button onClick={captureMedia}>Capture Video</button>
      </>
    );
}
export default Tab;