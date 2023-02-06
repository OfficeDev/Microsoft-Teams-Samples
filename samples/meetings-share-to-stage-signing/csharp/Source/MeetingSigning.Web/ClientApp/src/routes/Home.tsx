import { useEffect, useState } from 'react';
import { Header, Loader } from '@fluentui/react-northstar';
import { FrameContexts } from '@microsoft/teams-js';
import { useTeamsContext } from 'utils/TeamsProvider/hooks';
import { SidepanelDocumentCardList } from 'components/SidepanelDocumentCardList';
import { StageWithNoDocument } from 'components/StageWithNoDocument';
import { TabContent } from 'components/TabContent';
import styles from './Home.module.css';

export default function Home() {
  const context = useTeamsContext();
  const [showNotInTeamsError, setShowNotInTeamsError] =
    useState<boolean>(false);
  const timeoutWaitForTeamsToLoad = 5000;

  useEffect(() => {
    // Give Teams some time to get the context, but if it can't show an error.
    const timer = setTimeout(() => {
      setShowNotInTeamsError(true);
    }, timeoutWaitForTeamsToLoad);

    return () => clearTimeout(timer);
  }, [context]);

  switch (context?.page.frameContext) {
    case FrameContexts.meetingStage:
      return <StageWithNoDocument />;
    case FrameContexts.sidePanel:
      return <SidepanelDocumentCardList />;
    case FrameContexts.content:
      return <TabContent />;
    default:
      return (
        <div className={styles.homeLoader}>
          {!showNotInTeamsError && <Loader />}
          {showNotInTeamsError && (
            <Header
              as="h2"
              content="Unable to get information about the App."
              description="This happens if you are running the application in a normal browser, and not inside Teams.
                          Install the app inside Teams to test this application. To upload the app to Teams follow the instructions on https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/deploy-and-publish/apps-upload"
            />
          )}
        </div>
      );
  }
}
