/*!
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import * as microsoftTeams from "@microsoft/teams-js";
import { mergeClasses,Card,CardPreview,CardFooter } from "@fluentui/react-components";
import { IQuestionDetails } from '../types/question';
import { Image, Text, Button } from "@fluentui/react-components";
import { getFlexColumnStyles, getFlexItemStyles, getFlexRowStyles } from "../styles/layouts";

const SidePanel = () => {
    const flexRowStyle = getFlexRowStyles();

  const shareSpecificPart = (partName) => {
	microsoftTeams.app.initialize();
    var appContentUrl = "";
    appContentUrl = `${window.location.origin}/question/${partName}`;
    microsoftTeams.meeting.shareAppContentToStage((error, result) => {
      if (result) {
        // handle success
        console.log("success")
      }

      if (error) {
        // handle error
        console.log(JSON.stringify(error))
      }
    }, appContentUrl);
  };

  const flexColumnStyles = getFlexColumnStyles();
  const flexItemStyles = getFlexItemStyles();

  return (
    <div
      className={mergeClasses(
        flexColumnStyles.root,
        flexColumnStyles.fill,
        flexColumnStyles.vAlignStart,
        flexColumnStyles.scroll
      )}
    >
      <div
        className={mergeClasses(
          flexColumnStyles.root,
          flexColumnStyles.fill,
          flexColumnStyles.vAlignStart,
          flexColumnStyles.smallGap,
        )}
      >
        {IQuestionDetails.questions ? IQuestionDetails.questions.map((question) => {
          return <div key={`media-item-${question.questionId}`} className={flexItemStyles.noShrink}>
            <Card
              appearance="filled"
              style={{
                padding: "0rem",
                minHeight: "0rem",
                minWidth: "0px",
                width: "100%",
                height: "100%",
                margin: "0rem",
                cursor: "default",
                border: "none",
              }}
            >
              <CardPreview
                style={{
                  minHeight: "0px",
                  maxHeight: "140px",
                  overflow: "hidden",
                  marginBottom: "0.8rem",
                  padding: "2rem"
                }}
              >
                <div><Text size={400} weight="semibold">
                  {question.question}
                </Text></div>
              </CardPreview>
              <CardFooter
                styles={{ padding: "0px 4px", minHeight: "0px", minWidth: "0px" }}
              >
                <div className={flexRowStyle.smallGap}>
                  <Button
                    appearance="outline"
                    size="small"
                    onClick={() => {
                      shareSpecificPart(question.questionId);
                    }}
                  >
                    Share
                  </Button>
                </div>
              </CardFooter>
            </Card>
          </div>
        }) : <div> No content to show</div>}
      </div>

    </div>
  );
};

export default SidePanel;
