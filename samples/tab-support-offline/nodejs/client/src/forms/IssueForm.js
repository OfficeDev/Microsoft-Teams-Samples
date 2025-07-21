// IssueForm.js
import '../style/IssueForm.css';

import * as microsoftTeams from '@microsoft/teams-js';
import React, { useState, useEffect, useRef } from 'react';
import { Priority, Status } from '../models/FormsModel';
import { v4 as uuid } from "uuid";

import DropDown from '../components/DropDown';
import defaultIcon from '../assets/default-image.png';

const IssueForm = ({ onSave, onClose, selectedIssue, selectedFlight, actionHandler }) => {
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState(selectedIssue ? selectedIssue.priority : Priority[0]);
  const [status, setStatus] = useState(selectedIssue ? selectedIssue.status :Status[0]);
  const [image, setImage] = useState(null);
  const descriptionRef = useRef();

  useEffect(() => {
    descriptionRef.current.focus();
 }, []);

  useEffect(() => {
    // If a selected issue is provided, populate the form fields
    if (selectedIssue) {
      setDescription(selectedIssue.description || '');
      setPriority(selectedIssue.priority || '');
      setStatus(selectedIssue.status || '');  
      setImage(selectedIssue.image || null);
    }
  }, [selectedIssue]);

  const handleSave = () => {
    const updatedIssue = {
      uid: selectedIssue ? selectedIssue.uid : uuid(),
      description,
      priority,
      status,
      image
    };

    let errors = validateFields(updatedIssue);

    if (errors.length > 0) {
      alert(errors.join('\n'));
      return;
    }

    console.log(updatedIssue);
    onSave(updatedIssue);
  };

  function validateFields(updatedIssue) {
    let errors = [];
    if (updatedIssue.description === '') {
        errors.push('Description is required');
    }
    if (updatedIssue.status === '') {
        errors.push('Status is required');
    }
    if (updatedIssue.priority === '') {
        errors.push('Priority is required');
    }
    return errors;
}

  const selectImage = () => {
    microsoftTeams.media.selectMedia(
      {
        mediaType: microsoftTeams.media.MediaType.Image,
        maxMediaCount: 1,
        imageMaxWidth: 200,
        imageMaxHeight: 200
      },
      (error, attachments) => {
        if (error) {
          console.error('Error selecting image:', error);
          alert('Error selecting image:' + error)
          return;
        }

        if (attachments.length === 0) {
          console.log('No image attachment received');
          return;
        }

        const attachment = attachments[0];
        var src = "data:" + attachment.mimeType + ";base64," + attachment.preview;
        setImage(src);
      }
    );
  }

  return (
    <div className='issue-form-container' >
      <div className="issue-form-header">
        <h2>{selectedIssue ? 'Update Issue' : 'Create New Issue'}</h2>
        <button className="close-button" onClick={onClose}> &#9587; </button>
      </div>

      <div className="issue-form-body">
        
        <label htmlFor="description">Description:</label>
        <input
          type="text"
          id="description"
          ref={descriptionRef}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />

        <label htmlFor="priority">Priority:</label>
        <DropDown name="priority" options={Priority} onChange={(e) => setPriority(e.target.value)} value={priority} />

        <label htmlFor="status">Status:</label>
        <DropDown name="status" options={Status} onChange={(e) => setStatus(e.target.value)} value={status}/>

        <label htmlFor="image">Image:</label>
        
        <img id="image" 
          className={image == null ? 'default': ''} 
          src={image || defaultIcon} 
          alt="Issue" 
          onClick={(e) => selectImage(e.target.value)}
          style={{ height: '25%', width: '25%', objectFit: 'cover' }} 
        />

      </div>

      <div className="form-footer">
        <button className="save-button" onClick={handleSave}> Save </button>
      </div>
    </div>
  );
};

export default IssueForm;
