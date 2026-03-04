// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import {Text, Button, Card} from '@fluentui/react-components'
import { CardBody } from 'reactstrap';

/**
 * The 'ScanBarCode'
 * of your app.
 */
const ScanBarCode = () => {
  const [barCodeValue, setBarCodeValue] = useState('');
  useEffect(() => {
    microsoftTeams.app.initialize()
  })

  // Method to scan barcode
  function scanBarCode() {
    const config: microsoftTeams.media.BarCodeConfig = {
      timeOutIntervalInSec: 30
    };

    // Method that enables the user to scan different types of barcode, and returns the result as a string.  
    microsoftTeams.media.scanBarCode((error: microsoftTeams.SdkError, decodedText: string) => {
      // If there's any error, an alert shows the error message/code
      if (error) {
        if (error.message) {
          alert(" ErrorCode: " + error.errorCode + error.message);
        } else {
          alert(" ErrorCode: " + error.errorCode);
        }
      } else if (decodedText) {
        setBarCodeValue(decodedText);
      }
    }, config);
  }

  return (
    <>
      {/* Card for Barcode Scanner */}
      <Card>       
          <Text weight="bold">Scan Barcode (Mobile Only) </Text>       
        <CardBody>
          <div className='flex columngap'>
            <Text>Scan any barcode to get information related to it</Text>
            <Text weight="semibold">SDK used:</Text>
            <Text>microsoftTeams</Text>
            <Text weight="semibold" >Method:</Text>
            <Text>teams.media</Text>
            <Button onClick={scanBarCode} >Scan Barcode</Button>
          </div>
        </CardBody>
        {barCodeValue !== '' && <Text>Scanned Text: {barCodeValue}</Text>}
      </Card>
    </>
  );
}

export default ScanBarCode;