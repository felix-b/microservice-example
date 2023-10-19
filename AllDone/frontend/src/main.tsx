import React from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { App } from "./App";
import { Auth0ProviderWithNavigate } from "./providers/Auth0ProviderWithNavigate";
import "./styles/styles.css";

const container = document.getElementById("root");
const root = createRoot(container as Element);

root.render(
    <React.StrictMode>
        <BrowserRouter>
            <Auth0ProviderWithNavigate>
                <App />
            </Auth0ProviderWithNavigate>
        </BrowserRouter>
    </React.StrictMode>
);
