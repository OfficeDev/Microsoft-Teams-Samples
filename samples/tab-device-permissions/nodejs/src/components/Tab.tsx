// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { Flex } from '@fluentui/react-northstar'
import ScanBarCode from './ScanBarCode';
import GetGeoLocation from './GetGeoLocation';
import CaptureImage from './CaptureImage';
import PeoplePicker from './PeoplePicker';
import CaptureAudio from './CaptureAudio';

/**
 * The 'Tab' contains all the components
 * of your app.
 */
const Tab = () => {
    return(
      <Flex column gap="gap.large">
       {/* Component to capture image(s) */}
       <CaptureImage/>

       {/* Component to capture audio */}
       <CaptureAudio/>

      {/* Component to scan barcode */}
      <ScanBarCode/>

      {/* Component to show selected people */}
      <PeoplePicker/>

      {/* Component to Get/Show geo-Location */}
      <GetGeoLocation/>
      </Flex>
    );
}

export default Tab;