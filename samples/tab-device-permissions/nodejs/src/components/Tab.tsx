// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import { Grid, Segment } from '@fluentui/react-northstar'
import ScanBarCode from './ScanBarCode';
import GetGeoLocation from './GetGeoLocation';
import CaptureImage from './CaptureImage';
import PeoplePicker from './PeoplePicker';
import CaptureAudio from './CaptureAudio';
import GetLocationDesktop from './GetLocationDesktop';
import * as microsoftTeams from "@microsoft/teams-js";

/**
 * The 'Tab' contains all the components
 * of your app.
 */
const Tab = () => {
  const [isWeb, setIsWeb] = useState(false);

  useEffect(() => {
    // initializing microsoft teams sdk
    microsoftTeams.initialize();

    microsoftTeams.getContext((context) => {
      if (context.hostClientType! == "web") {
        setIsWeb(true);
      }
      else {
        setIsWeb(false);
      }
    })
  })

  return (
    <Grid columns={1}>
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
        <Segment
          /* Component to Get/Show geo-Location */
          content={<GetLocationDesktop />}
        />
      }
    </Grid>
  );
}

export default Tab;