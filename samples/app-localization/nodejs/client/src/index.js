import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';

import {I18nextProvider} from "react-i18next";
import i18next from "i18next";

import es_mx from "./translations/es-mx/common.json";
import en_us from "./translations/en-us/common.json";
import hi_in from "./translations/hi-in/common.json";

i18next.init({
  interpolation: { escapeValue: false }, 
  lng: 'en_us', 
  resources: {
    en_us: {
        common: en_us
    },
    hi_in: {
        common: hi_in
    },
    es_mx: {
        common: es_mx
    },
  },
});

ReactDOM.render(
  <React.StrictMode>
    <I18nextProvider i18n={i18next}>
      <App/>
    </I18nextProvider>
  </React.StrictMode>,
  document.getElementById('root')
);

reportWebVitals();
