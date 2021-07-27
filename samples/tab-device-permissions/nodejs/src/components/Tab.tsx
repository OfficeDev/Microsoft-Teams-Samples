// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { Grid, Segment } from '@fluentui/react-northstar'
import ScanBarCode from './ScanBarCode';
import GetGeoLocation from './GetGeoLocation';
import CaptureImage from './CaptureImage';
import PeoplePicker from './PeoplePicker';
import CaptureAudio from './CaptureAudio';

const getTabContent = () => {
  return (
    <Grid columns={window.innerWidth < 720 ? 1 : 5}>
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
    </Grid>
  )
}

/**
 * The 'Tab' contains all the components
 * of your app.
 */
const Tab = () => {
  return (
    getTabContent()
  );
}

export default Tab;