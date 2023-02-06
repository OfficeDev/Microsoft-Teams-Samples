// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import ScanBarCode from './ScanBarCode';
import GetGeoLocation from './GetGeoLocation';
import CaptureImage from './CaptureImage';
import CaptureImageDesktop from './CaptureImageDesktop';
import PeoplePicker from './PeoplePicker';
import CaptureAudio from './CaptureAudio';
import CaptureVideo from './CaptureVideo';
import CaptureAudioDesktop from './CaptureAudioDesktop';
import CaptureVideoDesktop from './CaptureVideoDesktop';
import GetLocationDesktop from './GetLocationDesktop';
import GetNotificationDesktop from './GetNotificationDesktop';
import * as microsoftTeams from "@microsoft/teams-js";
import Segment from 'react-segment-analytics';
/**
 * The 'Tab' contains all the components
 * of your app.
 */
const Tab = () => {
  const [isWeb, setIsWeb] = useState(false);

  useEffect(() => {
    // initializing microsoft teams sdk
    microsoftTeams.app.initialize().then(() => {
      microsoftTeams.app.getContext().then((context) => {
        if (context.app.host.clientType! === "web") {
          setIsWeb(true);
        }
        else {
          setIsWeb(false);
        }
      });
    });
  })
  
  const rowq = {
    rowd: {
      width:'100%',
      display: 'table', 
    },
  } as const;
  return (
   
    <div style={rowq.rowd} >
    {!isWeb &&
      <>
      <div className="Grid"> 
        <Segment
          /* Component to capture image(s) */
          children={<CaptureImage />} writeKey={''}
        />
        <Segment
          /* Component to Get/Show geo-Location */
          children={<GetGeoLocation />} writeKey={''}
        />
         <Segment 
          /* Component to capture audio */
          children={<CaptureAudio />} writeKey={''}
        />
        
      </div>
      <div>
         <Segment
          /* Component to scan barcode */
          children={<ScanBarCode />} writeKey={''}
        />
         <Segment
          /* Component to capture video */
          children={<CaptureVideo />} writeKey={''}
        />
         <Segment
          /* Component to show selected people */
          children={<PeoplePicker />} writeKey={''}
        />
      </div>
      </>
    }
    {isWeb &&
    <>
    <div className='Grid'>
    <Segment 
        /* Component to capture image in browser */
        children={<CaptureImageDesktop />} writeKey={''}
      />
      <Segment
        /* Component to Get/Show geo-Location in browser */
        children={<GetLocationDesktop />} writeKey={''}
      />
     
    </div>
      <div className='Grid'>
      <Segment 
        /* Component to capture audio in browser */
        children={<CaptureAudioDesktop />} writeKey={''}
      />
      <Segment 
        /* Component to Get notification in browser */
        children={<GetNotificationDesktop />} writeKey={''}
      />
     
      
      </div>
      <div className='Grid'>
      <Segment 
        /* Component to capture video in browser */
        children={<CaptureVideoDesktop />} writeKey={''}
       
      />
      </div>
      
      </>
    }
  </div>
  );
}

export default Tab;