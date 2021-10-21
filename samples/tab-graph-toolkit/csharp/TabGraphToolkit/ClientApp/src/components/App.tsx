import { Providers, ProviderState } from '@microsoft/mgt-element';
import { Agenda, Login, Todo, PersonCard, PeoplePicker, Tasks, Person, ViewType } from '@microsoft/mgt-react';
import React, { useState, useEffect } from 'react';
import { Accordion } from '@fluentui/react-northstar'
import '../App.css';

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
    const panels = [
        {
            key: 'Agenda',
            title: <div className="title">Agenda</div>,
            content: <Agenda />,
        },
        {
            key: 'Peoplepicker',
            title: <div className="title"> People picker </div>,
            content: <div className="container-div"> <PeoplePicker /></div>,
        },
        {
            key: 'Todo',
            title: <div className="title"> To do</div>,
            content: <div className="container-div"> <Todo /> </div>,
        },
        {
            key: 'PersonCard',
            title: <div className="title"> Person Card</div>,
            content: <div className="container-div"> <PersonCard personQuery="me" /> </div>,
        },
        {
            key: 'Person',
            title: <div className="title"> Person</div>,
            content: <div className="container-div"> <Person personQuery="me" view={ViewType.threelines} /> </div>,
        },
        {
            key: 'Tasks',
            title: <div className="title"> Tasks</div>,
            content: <div className="container-div"> <Tasks /></div>,
        },
    ]

    return (
        <div className="App">
            <header>
                <Login />
            </header>
            <div>
                {isSignedIn &&
                    <Accordion defaultActiveIndex={[0]} panels={panels}/>}
            </div>
        </div>
    );
}

export default App;