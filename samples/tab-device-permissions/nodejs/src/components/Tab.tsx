// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import { Grid, Segment } from '@fluentui/react-northstar'
import ScanBarCode from './ScanBarCode';
import GetGeoLocation from './GetGeoLocation';
import CaptureImage from './CaptureImage';
import CaptureImageDesktop from './CaptureImageDesktop';
import PeoplePicker from './PeoplePicker';
import CaptureAudio from './CaptureAudio';
import CaptureAudioDesktop from './CaptureAudioDesktop';
import CaptureVideoDesktop from './CaptureVideoDesktop';
import GetLocationDesktop from './GetLocationDesktop';
import GetNotificationDesktop from './GetNotificationDesktop';
import * as microsoftTeams from "@microsoft/teams-js";

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
        if (context.app.host.clientType! == "web") {
          setIsWeb(true);
        }
        else {
          setIsWeb(false);
        }
      });
    });
  })

  return (
    <Grid columns={isWeb ? 3 : 1}>
      {!isWeb &&
        <>
          <Segment
            /* Component to capture image(s) */
            content={<CaptureImage />}
          />
          <Segment
            /* Component to capture audio */
            content={<CaptureAudio />}
          />
          <Segment
            /* Component to scan barcode */
            content={<ScanBarCode />}
          />
          <Segment
            /* Component to show selected people */
            content={<PeoplePicker />}
          />
          <Segment
            /* Component to Get/Show geo-Location */
            content={<GetGeoLocation />}
          />
        </>
      }
      {isWeb &&
      <>
        <Segment
          /* Component to capture image in browser */
          content={<CaptureImageDesktop />}
        />
        <Segment
          /* Component to capture audio in browser */
          content={<CaptureAudioDesktop />}
        />
        <Segment
          /* Component to capture video in browser */
          content={<CaptureVideoDesktop />}
        />
        <Segment
          /* Component to Get/Show geo-Location in browser */
          content={<GetLocationDesktop />}
        />
        <Segment
          /* Component to Get notification in browser */
          content={<GetNotificationDesktop />}
        />
        </>
      }
    </Grid>
  );
}

export default Tab;