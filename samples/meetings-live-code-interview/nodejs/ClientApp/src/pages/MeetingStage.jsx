/*!
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import { useEffect, useRef, useState } from "react";
import { useParams } from "react-router-dom";
import * as microsoftTeams from "@microsoft/teams-js";
import { LiveShareHost } from "@microsoft/teams-js";
import { IQuestionDetails } from '../types/question';
import { SharedMap } from "fluid-framework";
import { LiveShareClient } from "@microsoft/live-share";
import { getLatestEditorValue, saveEditorState } from "./services/getLatestEditorValue"
import Editor from '@monaco-editor/react';

let containerValue;

const MeetingStage = (props) => {
  let { questionId } = useParams();
  let meetingId;
  const editorValueKey = "editor-value-key";

  const [data, setData] = useState("");

  useEffect(() => {
    microsoftTeams.app.initialize().then(() => {
      microsoftTeams.app.getContext().then((context) => {
        getLatestEditorValue(questionId, context.meeting.id).then((result) => {
          meetingId = context.meeting.id;
          if (result.data.value != null && result.data.value != "")
            setData(result.data.value);
        })
      })
    });
  }, []);

  // Initial setup for using fluid container.
  useEffect(() => {
    (async function () {
	  await microsoftTeams.app.initialize();
      let connection;
      window.localStorage.debug = "fluid:*";

    const host = LiveShareHost.create();
	
      // Define Fluid document schema and create container
      const client = new LiveShareClient(host);
      const containerSchema = {
        initialObjects: { editorMap: SharedMap }
      };

      function onContainerFirstCreated(container) {
        // Set initial state of the editorMap.
        container.initialObjects.editorMap.set(editorValueKey, 1);
      }

      // Joining the container with default schema defined.
      const { container } = await client.joinContainer(containerSchema, onContainerFirstCreated);
      containerValue = container;
      containerValue.initialObjects.editorMap.on("valueChanged", updateEditorState);
    })();
  }, []);

  // This method is called whenever the shared state is updated.
  const updateEditorState = () => {
    const editorValue = containerValue.initialObjects.editorMap.get(editorValueKey);
    var data = {
      meetingId: meetingId,
      questionId: questionId,
      editorData: editorValue
    }
    setData(editorValue);
    saveEditorState(data);
    alert(editorValue);
  };

  // Emit action to send message to server and update the shared state.
  const emitMessageAction = (handleClick, time) => {
    let timer
    return (...argument) => {
      clearTimeout(timer);
      timer = setTimeout(() => {
        handleClick.apply(this, argument);
      }, time)
    }
  }

  // Handler called when user types on the editor.
  const handleClick = emitMessageAction(async (value) => {
    var editorMap = containerValue.initialObjects.editorMap;
    editorMap.set(editorValueKey, value);
  }, 2000);

  return (
    <>
      {
        IQuestionDetails.questions ? IQuestionDetails.questions.map((question) => {
          if (question.questionId == questionId) {
            return <>
              <div>{question.question}</div>
              <div>{question.language}</div>
              <div>{question.expectedOuput}</div>
              <Editor
                height="80vh"
                defaultLanguage={question.language}
                defaultValue={question.defaultValue}
                value={data}
                onChange={handleClick}
              />
            </>
          }
        }) : null}
    </>
  );
};

export default MeetingStage;