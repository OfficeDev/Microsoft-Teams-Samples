import {
  createContext,
  Dispatch,
  SetStateAction,
  useEffect,
  useState,
} from 'react';
import * as microsoftTeams from '@microsoft/teams-js';

export enum AuthState {
  Unrequested,
  Pending,
  Resolved,
  Rejected,
  Expired,
}

type AuthContextType = {
  token: string | undefined;
  state: AuthState;
  setToken: Dispatch<SetStateAction<string | undefined>>;
  setState: Dispatch<SetStateAction<AuthState>>;
};

const AuthContext = createContext<AuthContextType>({} as AuthContextType);

const AuthProvider: React.FC = ({ children }) => {
  const [token, setToken] = useState<string | undefined>();
  const [authState, setAuthState] = useState<AuthState>(AuthState.Unrequested);

  useEffect(() => {
    microsoftTeams.initialize();
  }, []);

  useEffect(() => {
    const getAuthTokenFromTeams = () => {
      // Perform single sign-on authentication
      setAuthState(AuthState.Pending);
      microsoftTeams.authentication.getAuthToken({
        successCallback: (result: string) => {
          setAuthState(AuthState.Resolved);
          setToken(result);
        },
        failureCallback: (error: string) => {
          setAuthState(AuthState.Rejected);
          setToken(undefined);
          ssoLoginFailure(error);
        },
      });
    };

    if (
      authState === AuthState.Unrequested ||
      authState === AuthState.Expired
    ) {
      getAuthTokenFromTeams();
    }
  }, [authState]);

  const ssoLoginFailure = (error: string) => {
    console.error('SSO failed: ', error);
  };

  return (
    <AuthContext.Provider
      value={{
        token: token,
        state: authState,
        setToken: setToken,
        setState: setAuthState,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export { AuthProvider, AuthContext };
