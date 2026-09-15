import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom'; // 🟢 BrowserRouter yahan import kiya
import App from './App.jsx';
import './index.css';
import { AuthProvider } from './context/AuthContext.jsx'; 
import * as Sentry from "@sentry/react";

Sentry.init({
  dsn: "https://examplePublicKey@o0.ingest.sentry.io/0", // Ye dummy hai
  integrations: [
    Sentry.browserTracingIntegration() 
  ],
  tracesSampleRate: 1.0,
});

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    {/* 🟢 AuthProvider ko BrowserRouter ke ANDAR rakha */}
    <BrowserRouter>
      <AuthProvider>
        <App />
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);