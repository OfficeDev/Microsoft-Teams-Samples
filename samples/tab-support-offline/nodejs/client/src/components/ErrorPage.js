import React from 'react';
import {getFullPageErrorInfo} from '../models/ErrorModel';

function ErrorPage({ fullpageError, actionHandler, loading}) {
    const { icon, title, description, actionTitle } = getFullPageErrorInfo(fullpageError);

    if (loading) {
        return (
          <div className='spinner-container'>
            <p>Loading...</p>
            <div className='spinner'></div>
          </div>
        );
      }

    return (
        <div className="error-page">
            <img src={icon} alt="error icon" />
            <h1>{title}</h1>
            <p>{description}</p>
            {actionTitle && <button onClick={actionHandler}>{actionTitle}</button>}
        </div>
    );
}

export default ErrorPage;