// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { useEffect, useState } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Button, Card } from '@fluentui/react-components'
import { CardBody } from 'reactstrap';

/**
 * The 'PeoplePicker' component for 
 * of your app.
 */
const PeoplePicker = () => {
  const [selectedPeople, setSelectedPeople] = useState<microsoftTeams.people.PeoplePickerResult[]>([]);
  useEffect(() => {
    microsoftTeams.app.initialize()
  })

  // Method to select people using people picker control
  function selectPeople() {
    microsoftTeams.people.selectPeople().then((people) => {
      setSelectedPeople(people);
    }).catch((error) => {
      if (error.message) {
        alert(" ErrorCode: " + error.errorCode + error.message);
      }
      else {
        alert(" ErrorCode: " + error.errorCode);
      }
    });
  }

  return (
    <>
      {/* Card for People Picker */}
      <Card>       
          <Text weight="bold">People Picker (Mobile Only)</Text>        
        <CardBody>
          <div className='flex columngap'>
            <Text weight="semibold">SDK used:</Text>
            <Text>microsoftTeams</Text>
            <Text weight="semibold" >Method</Text>
            <Text>teams.people</Text>
            <Button onClick={selectPeople} > People Picker</Button>
          </div>
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