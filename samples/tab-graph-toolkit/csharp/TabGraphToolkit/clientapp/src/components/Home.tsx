import { useState, useEffect } from 'react';
import { Agenda, Login } from '@microsoft/mgt-react';
import { Providers, ProviderState } from '@microsoft/mgt';

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

function Home () {
    const [isSignedIn] = useIsSignedIn();
    return (
        <div className="config-container">
            <header>
                <Login />
            </header>
            <div>
                {isSignedIn &&
                    <Agenda />}
            </div>
        </div>
    )
}

export default Home;