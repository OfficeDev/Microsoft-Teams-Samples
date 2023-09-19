import React from "react";
import App from "./components/App";
import { createRoot } from "react-dom/client";

/* global document, Office */

const rootElement: HTMLElement = document.getElementById("root");

const root = createRoot(rootElement);

Office.onReady(() => {
  root.render(
    <React.StrictMode>
      <App />
    </React.StrictMode>
  );
});
