import React, { useState, useEffect, useContext } from 'react';
import IssueForm from '../../forms/IssueForm';
import { FullPageError } from '../../models/ErrorModel';
import ErrorPage from '../ErrorPage';
import IssueTable from './IssueTable';
import IncidentRepository from '../../db/IncidentRepository';
import { TeamsContext } from '../../Main';

function StagedIssuesTab(props) {
  const [stagedIssues, setStagedIssues] = useState([]);
  const [syncingIssues, setSyncingIssues] = useState(new Set());
  const [selectedIssue, setSelectedIssue] = useState(null);
  const [isFomVisible, setFormVisibility] = useState(false);
  const [shouldRerender, setShouldRerender] = useState(false);
  const incidentRepo = new IncidentRepository();
  const teamsToken = useContext(TeamsContext).token;
  const {selectedFlight} = props;

  /// Form Visibility

  const showForm = (issue) => {
    setSelectedIssue(issue);
    setFormVisibility(true);
  };

  const hideForm = (shouldRefresh) => {
    setSelectedIssue(null);
    setFormVisibility(false);
    
    if (shouldRefresh) {
      setShouldRerender(!shouldRerender);
    }
  };

  //// Actions

  const handleCreateNewIssue = (newIssue) => {
    newIssue.flightId = selectedFlight;
    let promise;
    if (selectedIssue) {
      promise = incidentRepo.updateRecordById(selectedIssue.id, newIssue);
    } else if (newIssue) {
      promise = incidentRepo.saveRecord(newIssue);
    }

    promise.then(() => {
      hideForm(true);
    });
  }

  function dataURItoBlob(dataURI) {
    const splitDataURI = dataURI.split(",");
    const mimeString = splitDataURI[0].split(":")[1].split(";")[0];
    const byteString = atob(splitDataURI[1]);
  
    // Create a Uint8Array from the base64 string
    const arrayBuffer = new ArrayBuffer(byteString.length);
    const uint8Array = new Uint8Array(arrayBuffer);
  
    for (let i = 0; i < byteString.length; i++) {
      uint8Array[i] = byteString.charCodeAt(i);
    }
  
    return new Blob([arrayBuffer], { type: mimeString });
  }

  const handleSync = async () => {
    // Iterate over stagedIssues and post each issue to "/api/issues"
    let allIssues = await incidentRepo.getRecords();
    for (const stagedIssue of allIssues) {
      try {
        // Start syncing, add issue to syncingIssues
        setSyncingIssues((prevSyncingIssues) => new Set([...prevSyncingIssues, stagedIssue.id]));

        if(stagedIssue.image !== null) {
          const blobName = stagedIssue.uid + ".jpg";
          const containerName = process.env.REACT_APP_CONTAINERNAME;
          const blobServiceUrl = process.env.REACT_APP_BLOBSERVICEURL;
          //todo: get this from server
          const sasToken = process.env.REACT_APP_SASTOKEN;
          const blobUrlWithSAS = `${blobServiceUrl}/${containerName}/${blobName}?${sasToken}`;  
          const blob = dataURItoBlob(stagedIssue.image);
          let blobRespose = await fetch(blobUrlWithSAS, {
            method: 'PUT',
            headers: {
              'x-ms-blob-type': 'BlockBlob',
              'Content-Type': blob.type,
            },
            body: blob
          });

          if(blobRespose.ok) {
            stagedIssue.image = blobUrlWithSAS;
          }
        }

        const response = await fetch(`/api/flights/${selectedFlight}/issues`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${teamsToken}`,
          },
          body: JSON.stringify(stagedIssue),
        });

        if (response.ok) {
          // If successful, delete the issue from incidentRepo
          incidentRepo.deleteRecord(stagedIssue.id);
          refreshRecords();
        } else {
          console.error(`Failed to sync issue ${stagedIssue.id}. Status: ${response.status}`);
        }        
      } catch (error) {
        console.error(`Error syncing issue ${stagedIssue.id}:`, error);
      } finally {
        // Stop syncing, remove issue from syncingIssues
        setSyncingIssues((prevSyncingIssues) => {
          const newSyncingIssues = new Set(prevSyncingIssues);
          newSyncingIssues.delete(stagedIssue.id);
          return newSyncingIssues;
        });
      }
    }
  };

  useEffect(() => {
    refreshRecords();
  }, [shouldRerender, selectedFlight]);


  const refreshRecords = () => {
    incidentRepo.getFlightRecords(selectedFlight).then((issues) => {
      setStagedIssues(issues);
    })
  }

  //// UI Elements

  const header = <div>
    <div className='hint-box'>
      <p> Issues that have been created or updated but not yet saved. Click on an issue to edit it.</p>
    </div>
    <br />
    <button onClick={e => showForm(null)}> Create New Issue</button>
    <button onClick={handleSync} style={{ float: 'right' }}> Sync</button>
  </div>

  const form = isFomVisible && <IssueForm onSave={issue => handleCreateNewIssue(issue)} onClose={hideForm} selectedIssue={selectedIssue} selectedFlight={selectedFlight}/>;

  //// Emtpy State

  if (stagedIssues?.length === 0) {
    return <div>
      {header}
      <ErrorPage fullpageError={FullPageError.NO_DATA} actionHandler={e => showForm(null)} />;
      {form}
    </div>
  }

  //// Data Handling

  const handleDeleteStagedIssue = (issueId) => {
    incidentRepo.deleteRecord(issueId);
    setShouldRerender(!shouldRerender);
  };

  const rowUpdateAction = {
    title: '\u2715',
    actionHandler: (issue) => {
      handleDeleteStagedIssue(issue.id);
    }
  };

  const stagedIssuesPage = () => {
    return (
      <div>
        {header}
        <IssueTable issues={stagedIssues} onRowTap={e => showForm(e)} rowUpdateAction={rowUpdateAction} syncingIssues={syncingIssues} />
        {form}
      </div>
    );
  };

  return stagedIssuesPage();
}

export default StagedIssuesTab;