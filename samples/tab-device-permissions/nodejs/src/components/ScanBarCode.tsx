// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody } from '@fluentui/react-northstar'

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
        <CardHeader>
          <Text content="Scan Barcode (Mobile Only)" weight="bold" />
        </CardHeader>
        <CardBody>
          <Flex column gap="gap.small">
            <Text content="Scan any barcode to get information related to it" />
            <Text content="SDK used: " weight="semibold" />
            <Text content="microsoftTeams" />
            <Text content="Method: " weight="semibold" />
            <Text content="teams.media" />
            <Button content="Scan Barcode" onClick={scanBarCode} />
          </Flex>
        </CardBody>
        {barCodeValue !== '' && <Text>Scanned Text: {barCodeValue}</Text>}
      </Card>
    </>
  );
}

export default ScanBarCode;