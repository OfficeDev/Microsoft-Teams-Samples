import React, { useState } from 'react';
import { Route, Switch } from 'react-router';
import PersonalApp from 'components/PersonalApp';
import SharedApp from 'components/SharedApp';
import TaskModule from 'components/TaskModule';
import './index.css';
import ConfigTab from 'components/ConfigTab/ConfigTab';
import { AppInsightsErrorBoundary } from '@microsoft/applicationinsights-react-js';
import { reactPlugin } from 'appInsights';
import { IntlProvider } from 'react-intl';
import { loadCompositionRoot, QBotApplication } from 'compositionRoot';
import { Provider as ReduxProvider } from 'react-redux';
import { ConnectedRouter } from 'connected-react-router';
import { useEffect } from 'react';
import {
  useNotifyAppLoaded,
  useNotifySuccess,
  useTeamsContext,
} from 'components/TeamsProvider/hooks';
import { parseLocale } from 'localization/localizationService';
import { ErrorComp } from 'components/TaskModule/ErrorComp';
import { SubEntityRouter } from 'subEntityRouting/SubEntityRouter';

export const QBotContext = React.createContext<QBotApplication>(
  // Set the default context to undefined,
  // this is fine as it will be set when the application mounts
  // and saves us from numerous unnecessary null-checks
  (undefined as unknown) as QBotApplication,
);

// eslint-disable-next-line max-lines-per-function
export default function App(): JSX.Element {
  const [qBotApplication, setQBotApplication] = useState<
    QBotApplication | undefined
  >();
  const teamsContext = useTeamsContext();
  const notifyAppLoaded = useNotifyAppLoaded();
  const notifySuccess = useNotifySuccess();
  useEffect(() => {
    if (!teamsContext.locale) return;
    loadCompositionRoot(parseLocale(teamsContext.locale)).then(
      (application) => {
        setQBotApplication(application);
        // queue up that the app has loaded
        // don't execute now since that will pre-empt the react render.
        setTimeout(() => {
          notifyAppLoaded();
          notifySuccess();
        }, 0);
      },
    );
  }, [teamsContext.locale, setQBotApplication, notifyAppLoaded, notifySuccess]);

  // While loading, just show nothing
  // Teams will show the loading indicator and we'll tell it to clear when we have the application loaded
  if (qBotApplication === undefined) {
    return <></>;
  }

  const { locale, messages, store, history } = qBotApplication;

  return (
    <QBotContext.Provider value={qBotApplication}>
      <IntlProvider locale={locale} messages={messages}>
        <ReduxProvider store={store}>
          <ConnectedRouter history={history}>
            <AppInsightsErrorBoundary
              onError={() => <ErrorComp />}
              appInsights={reactPlugin}
            >
              <SubEntityRouter />
              <Switch>
                <Route path="/personal/*">
                  <PersonalApp />
                </Route>
                <Route path="/configTab/*">
                  <ConfigTab />
                </Route>
                <Route path="/share/*">
                  <SharedApp />
                </Route>
                <Route path="/taskmodule/*">
                  <TaskModule />
                </Route>
              </Switch>
            </AppInsightsErrorBoundary>
          </ConnectedRouter>
        </ReduxProvider>
      </IntlProvider>
    </QBotContext.Provider>
  );
}
