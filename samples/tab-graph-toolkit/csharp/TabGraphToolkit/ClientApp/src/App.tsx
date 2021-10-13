import { Providers, ProviderState } from '@microsoft/mgt-element';
import { Agenda, Login, Todo, PersonCard, PeoplePicker } from '@microsoft/mgt-react';
import React, { useState, useEffect } from 'react';
import './App.css';

function useIsSignedIn(): [boolean] {
  const [isSignedIn, setIsSignedIn] = useState(false);

  useEffect(() => {
    const updateState = () => {
      const provider = Providers.globalProvider;
      setIsSignedIn(provider && provider.state === ProviderState.SignedIn);
    };

    Providers.onProviderUpdated(updateState);
    updateState();

    return () => {
      Providers.removeProviderUpdatedListener(updateState);
    }
  }, []);

  return [isSignedIn];
}

function App() {
  const [isSignedIn] = useIsSignedIn();

  return (
    <div className="App">
      <header>
        <Login />
      </header>
      <div>
        {isSignedIn &&
          <div>
            <div className="title">Agenda</div>
            <Agenda />
            <div className="title"> People picker </div>
            <PeoplePicker />
            <div className="title"> To do</div>
            <Todo />
            <div className="title"> Person Card</div>
            <PersonCard personQuery="me" />
          </div>}
      </div>
    </div>
  );
}

export default App;