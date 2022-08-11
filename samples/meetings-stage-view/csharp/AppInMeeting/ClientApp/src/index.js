import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import { AppRoute } from './router/router';

ReactDOM.render(
    <React.StrictMode>
        <AppRoute />
    </React.StrictMode>, document.getElementById('root')
);