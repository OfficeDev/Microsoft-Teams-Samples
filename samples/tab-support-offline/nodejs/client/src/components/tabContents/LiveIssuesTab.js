import React, { useState, useEffect, useContext } from 'react';
import { TeamsContext } from '../../Main';
import { FullPageError } from '../../models/ErrorModel';
import ErrorPage from '../ErrorPage';
import IssueTable from './IssueTable';

function LiveIssuesTab(props) {
  const [apiFetchError, setAPIFetchError] = useState(null); // TODO: use this to show error message
  const [serverIssues, setServerIssues] = useState([]);
  const [isNetworkAvailable, setNetworkAvailability] = useState(true);
  const [isLoading, setLoading] = useState(false); 
  const teamsToken = useContext(TeamsContext).token;
  const {selectedFlight} = props;


  const fetchServerIssues = (token) => {
    setLoading(true);
    setServerIssues([]);
    let authHeader = 'Bearer ' + token;
    console.log(authHeader);
    fetch(`/api/flights/${selectedFlight}/issues`, { headers: { Authorization: authHeader } })
      .then((res) => {
        if (res.ok) {
          return res.json();
        } else if (res.status === 401) {
          throw new Error('UnAuthorized');
        } else {
          throw new Error('Unexpected error');
        }
      })
      .then((data) => {
        setLoading(false);
        setServerIssues(data);
      })
      .catch((error) => {
        setLoading(false);
        if (error.message === 'UnAuthorized') {
          // Handle UnAuthorized error (show error message, redirect, etc.)
          console.log('UnAuthorized: TODO: show this error to the user');
          setAPIFetchError(error.message);
        } else {
          // Handle other errors
          console.error('Error fetching server issues:', error.message);
          setAPIFetchError(error.message);
        }
      });
  };

  useEffect(() => {
    // Fetch server issues initially
    fetchServerIssues(teamsToken);
  }, [teamsToken, selectedFlight]);


  useEffect(() => {
    const handleOnline = async () => {
      setNetworkAvailability(navigator.onLine);
      if(navigator.onLine) {
        await fetchServerIssues(teamsToken);
      };  
    };

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOnline);

    // Clean up event listener on component unmount
    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOnline);
    };
  }, []);


  const handleRetry = () => {
    console.log('TODO: handle retry');
  };

  const liveIssuesPage = () => {
    return (
      <div> 
        {isLoading ? (
          <div className='spinner-container'>
            <p>Loading...</p>
            <div className='spinner'></div>
          </div>
        ) : (
          <>
            <div className='hint-box'>
              <p>Issues that have been fetched from the server.</p>
            </div>
            <br/>
            <IssueTable issues={serverIssues} />
          </>
        )}
      </div>
      );
  };

  if (!isNetworkAvailable) {
    return <ErrorPage fullpageError={FullPageError.NO_INTERNET} actionHandler={handleRetry} />;
  } else if (apiFetchError != null || serverIssues.length === 0) {
    return <ErrorPage fullpageError={FullPageError.NO_DATA} actionHandler={handleRetry} loading={isLoading} />;
  } else {
    return liveIssuesPage();
  }
}

export default LiveIssuesTab;
