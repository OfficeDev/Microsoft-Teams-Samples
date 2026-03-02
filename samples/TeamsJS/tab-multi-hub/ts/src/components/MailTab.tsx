// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { app, mail } from "@microsoft/teams-js";
import validator from 'validator';

// function for composing mail
function MailTab() {
  const [to, setTo] = useState<string[]>([]);
  const [cc, setCC] = useState<string[]>([]);
  const [subject, setSubject] = useState("");
  const [body, setBody] = useState("");
  const [emailError, setEmailError] = useState(false);
  const [emailCcError, setEmailCcError] = useState(false);
  const [inputSubError, setInputSubError] = useState(false);
  const [inputBodyError, setInputBodyError] = useState(false);
  const [platformIsSupported, setplatformIsSupported] = useState(false);

  useEffect(() => {
    app.initialize();
  });

  // Set value for To Recepients.
  const functionRecepients = (e: any) => {
    let email = e.target.value;
    let flag: boolean = true;
    const toEmailList: [] = email.split(";");

    // checking valid email from the list 
    toEmailList.map(e => {
      
      if (validator.isEmail(e)) {
        return;
      }
      else {
        flag = false;
        return;
      }
    });

    if (flag === true) {
      setEmailError(false);
      setTo(toEmailList);
    }
    else {
      setEmailError(true);
    }
  }

  // Set value for Cc Recepients
  const functionCcRecepients = (e: any) => {
    let email = e.target.value;
    let flag: boolean = true;
    const CcEmailList: [] = email.split(";");

    // checking valid email from the list 
    CcEmailList.map(e => {
      if (validator.isEmail(e)) {
        return;
      }
      else {
        flag = false;
        return;
      }
    });

    if (flag === true) {
      setEmailCcError(false);
      setCC(CcEmailList);
    }
    else {
      setEmailCcError(true);
    }
  }

  // Set value for email Subject
  const functionSubject = (e: any) => {
    let subject = e.target.value;

    if (!validator.isLength(subject, { min: 3 })) {
      setInputSubError(true);
    }
    else {
      setInputSubError(false);
      setSubject(subject);
    }
  }

  // Set value for email body.
  const functionBody = (e: any) => {
    let body = e.target.value;

    if (!validator.isLength(body, { min: 4 })) {
      setInputBodyError(true);
    }
    else {
      setInputBodyError(false);
      setBody(body)
    }
  }

  // Handling the mail values on button click Compose Mail using handleSubmit function
  const handleSubmit = () => {
    var ComposeNewParams: mail.ComposeNewParams =
    {
      toRecipients: to,
      ccRecipients: cc,
      subject: subject,
      message: body,
      type: mail.ComposeMailType.New
    }

    // Check platform is supported or not
    if (mail.isSupported()) {
      mail.composeMail(ComposeNewParams);
    }
    else {
      setplatformIsSupported(true);
    }
  }

  return (
    <div className="mailDiv moduleDiv">
      <h3 className="hMailModule">Mail Module </h3>
      <form onSubmit={handleSubmit}>
        <label>To</label>
        <input className="inputValue" type="text" onChange={(e) => functionRecepients(e)} name="to" placeholder="To Recepients" required />
        {emailError ? <span style={{ color: 'red' }}>Please enter valid Email</span> : ""}

        <br></br>
        <label>Cc</label>
        <input className="inputValue"  type="text" onChange={(e) => functionCcRecepients(e)} name="lastname" placeholder="Cc Recepients" required />
        {emailCcError ? <span style={{ color: 'red' }}>Please enter valid Email</span> : ""}

        <br></br>
        <label>Subject</label>
        <input className="inputValue"  type="text" onChange={(e) => functionSubject(e)} name="subject" placeholder="Enter Subject" required />
        {inputSubError ? <span style={{ color: 'red' }}>Subject must be grater than 5 characters</span> : ""}

        <br></br>
        <label>Body</label>
        <textarea className="txtBody" onChange={(e) => functionBody(e)} name="body" placeholder="Enter Body" required />
        {inputBodyError ? <span style={{ color: 'red' }}>Body must be grater than 5 characters</span> : ""}
        <br></br>

        <input className="btnSubmit" type="submit" value="Compose Mail" />
        {platformIsSupported ? <span style={{ color: 'red',marginLeft:2 }}>Sorry, This app is currently not supported on this platform.</span> : ""}
      </form>
      <p className="note"><b>Note :</b> Please use semi-colon <b>" ; "</b> only for multiple email address</p>
    </div>
  );
}
export default MailTab;