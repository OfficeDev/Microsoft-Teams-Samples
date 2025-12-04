import { useEffect, useState } from 'react';
import CaptureImage from './CaptureImage';
import CaptureImageWeb from './CaptureImageWeb';
import PeoplePicker from './PeoplePicker';
import CaptureAudioWeb from './CaptureAudioWeb';
import CaptureVideoWeb from './CaptureVideoWeb';
import GetLocationWeb from './GetLocationWeb';
import GetNotificationWeb from './GetNotificationWeb';
import * as microsoftTeams from "@microsoft/teams-js";

/**
 * The 'Tab' contains all the components of your app.
 */
const Tab = () => {
  const [isWeb, setIsWeb] = useState(false);

  useEffect(() => {
    // initializing microsoft teams sdk
    microsoftTeams.app.initialize().then(() => {
      microsoftTeams.app.getContext().then((context) => {
        setIsWeb(context.app.host.clientType === "web");
      });
    });
  }, []); // Added dependency array to prevent infinite loop

  const containerStyle = {
    width: '100%',
    display: 'table',
  };

  return (
    <div style={containerStyle}>
      {!isWeb && (
        <>
          <div className="Grid">
            {/* Component to capture image(s) */}
            <CaptureImage />

            {/* 
            // Optional: Uncomment these if needed and stable

            // <GetGeoLocation />
            // <CaptureAudio />
            */}
          </div>
          <div className="Grid">
            {/*
            // Component to scan barcode
            // <ScanBarCode />

            // Component to capture video
            // <CaptureVideo />
            */}

            {/* Component to show selected people */}
            <PeoplePicker />
          </div>
        </>
      )}
      {isWeb && (
        <>
          <div className="Grid">
            {/* Component to capture image in browser */}
            <CaptureImageWeb />

            {/* Component to Get/Show geo-Location in browser */}
            <GetLocationWeb />
          </div>
          <div className="Grid">
            {/* Component to capture audio in browser */}
            <CaptureAudioWeb />

            
            {/* // Component to Get notification in browser */}
             <GetNotificationWeb />
           
          </div>
          <div className="Grid">
            {/* Component to capture video in browser */}
            <CaptureVideoWeb />
          </div>
        </>
      )}
    </div>
  );
};

export default Tab;
