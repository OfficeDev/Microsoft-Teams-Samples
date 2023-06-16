import React,{ useEffect, useState } from 'react';
import { app,calendar,authentication } from "@microsoft/teams-js";
import './App.css';

function Tab() {
  const [stateDatetime, setStartDatetime] = useState('');  // useState to store StateDatetime
  const [endDatetime, setEndDatetime] = useState('');  // useState to store EndDatetime
  const [attendees, setAttendees] = useState('');  // useState to store Attendees
  const [subject, setsubject] = useState('');  // useState to store Subject
  const [content, setContent] = useState('');  // useState to store Content
  const [token, settoken] = useState('');  // useState to store token

  useEffect(() => {
    app.initialize();
  })


  function startHandleChange(ev: any) {
    if (!ev.target['validity'].valid) return;
    const dt = ev.target['value'] + ':00Z';
    setStartDatetime(dt);
  }
  function endHandleChange(ev: any) {
    if (!ev.target['validity'].valid) return;
    const dt = ev.target['value'] + ':00Z';
    setEndDatetime(dt);
  }
  function subjectHandleChange(ev: any) {
    const subjectData = ev.target.value;
    setsubject(subjectData);
  }
  function attendeesHandleChange(ev: any) {
    const attendeesData = ev.target.value;
    setAttendees(attendeesData);
  }
  function contentHandleChange(ev: any) {
    const setContentData = ev.target.value;
    setContent(setContentData);
  }
  function composeMeeting() {
    var isValidation = false;
    if ((subject !== undefined && subject !== null && subject !== "") &&
      (content !== undefined && content !== null && content !== "") &&
      (attendees !== undefined && attendees !== null && attendees !== "") &&
      (stateDatetime !== undefined && stateDatetime !== null && stateDatetime !== "") &&
      (endDatetime !== undefined && endDatetime !== null && endDatetime !== "")) {
      isValidation = true;
    }
    if (isValidation == true) {
      var ComposeMeetingParams =
      {
        attendees: [attendees],
        startTime: stateDatetime, // MM/DD/YYYY HH:MM:SS format
        endTime: endDatetime,  // MM/DD/YYYY HH:MM:SS format
        subject: subject,
        content: content
      }
      calendar.composeMeeting(ComposeMeetingParams);
    }
  }
  return (
    <div>
      <div className='mainDiv'>
        <h2>Add Details for Compose Meeting:</h2>
        <form name="ComposeMeetingForm">
          <table>
            <tr>
              <td><label className='lblText'>Attendees:</label></td>
              <td> <input type="text" className='txtValue' placeholder="Attendees" required value={(attendees || '').toString()} onChange={attendeesHandleChange} /></td>
            </tr>
            <tr>
              <td><label className='lblText'>Start Time:</label></td>
              <td><input type="datetime-local" className='dateTimeValue' required value={(stateDatetime || '').toString().substring(0, 16)} onChange={startHandleChange} /></td>
            </tr>
            <tr>
              <td><label className='lblText'>End Time:</label></td>
              <td><input type="datetime-local" className='dateTimeValue' required value={(endDatetime || '').toString().substring(0, 16)} onChange={endHandleChange} /></td>
            </tr>
            <tr>
              <td> <label className='lblText'>Subject:</label></td>
              <td> <input type="text" className='txtValue' placeholder="Subject" value={(subject || '').toString()} onChange={subjectHandleChange} required /></td>
            </tr>
            <tr>
              <td> <label className='lblText'>Content:</label></td>
              <td><textarea required  className='dateTimeValue' placeholder="Type your meeting content..." rows={4} cols={40} value={(content || '').toString()} onChange={contentHandleChange} /></td>
            </tr>
            <tr>
              <td></td>
              <td><button onClick={composeMeeting}>Compose Meeting!</button></td>
            </tr>
          </table>
        </form>
      </div>
      <div>
      <h2 className='mainDivOpen'>Opens a calendar item:</h2>
      <table id="calendar">
        <tr>
          <th>Subject</th>
          <th>BodyPreview</th>
          <th>Event</th>
        </tr>
        <tr>
          <td>Microsoft Build 2023 - Mesh for Teams - Microsoft Metaverse</td>
          <td>Dear All,Here is the Agenda for the session....</td>
          <td><button onClick={composeMeeting}>Calendar!</button></td>
        </tr>
      </table>
      </div>
    </div>
  );
}
export default Tab;