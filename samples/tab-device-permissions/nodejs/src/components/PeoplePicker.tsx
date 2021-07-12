// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Card, Flex, Text, Button, CardHeader, CardBody } from '@fluentui/react-northstar'

/**
 * The 'PeoplePicker' component for 
 * of your app.
 */
const PeoplePicker = () => {
  const [selectedPeople, setSelectedPeople] = useState<microsoftTeams.people.PeoplePickerResult[]>([]);
  useEffect(() => {
    microsoftTeams.initialize()
  })

  // Method to select people using people picker control
  function selectPeople() {
    microsoftTeams.people.selectPeople((error: microsoftTeams.SdkError, people: microsoftTeams.people.PeoplePickerResult[]) => {
      if (error) {
        if (error.message) {
          alert(" ErrorCode: " + error.errorCode + error.message);
        }
        else {
          alert(" ErrorCode: " + error.errorCode);
        }
      }
      if (people) {
        setSelectedPeople(people);
      }
    });
  }

  return (
    <>
      {/* Card for People Picker */}
      <Card>
        <CardHeader>
          <Text content="People Picker (Mobile Only)" weight="bold" />
        </CardHeader>
        <CardBody>
          <Flex column gap="gap.small">
            <Button content="People Picker" onClick={selectPeople} />
          </Flex>
          <Text>Selected {selectedPeople.length} people</Text>
          {selectedPeople.length !== 0 && selectedPeople.map((item: microsoftTeams.people.PeoplePickerResult, index) => {
            return (
              <Text>{item.displayName}</Text>
            )
          })}
        </CardBody>
      </Card>
    </>
  );
}

export default PeoplePicker;