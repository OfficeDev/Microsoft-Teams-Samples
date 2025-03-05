import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';

// Import necessary i18n libraries
import { I18nextProvider } from "react-i18next";
import i18next from "i18next";

// Import translation files for multiple languages
import es_mx from "./translations/es-mx/common.json";
import en_us from "./translations/en-us/common.json";
import hi_in from "./translations/hi-in/common.json";

/**
 * Initializes i18next with the supported languages and resources.
 * Sets the default language to 'en_us'.
 */
i18next.init({
  interpolation: { escapeValue: false }, // Disable escaping as we don't need to escape the strings
  lng: 'en_us', // Set the default language to 'en_us'
  resources: {
    en_us: {
      common: en_us, // English translations
    },
    hi_in: {
      common: hi_in, // Hindi translations
    },
    es_mx: {
      common: es_mx, // Spanish translations
    },
  },
});

// Render the app wrapped inside the I18nextProvider for internationalization support
ReactDOM.render(
  <React.StrictMode>
    <I18nextProvider i18n={i18next}>
      <App />
    </I18nextProvider>
  </React.StrictMode>,
  document.getElementById('root') 
);

// Log performance metrics (optional)
reportWebVitals();
