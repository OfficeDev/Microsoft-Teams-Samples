/**
 * Config Component - JavaScript equivalent of React Config.js
 * Handles Teams tab configuration
 */

window.Config = (function() {
    'use strict';
    
    function render() {
        const container = document.createElement('div');
        container.className = 'container mt-4';
        
        container.innerHTML = `
            <div>
                <div style="display: flex; font-size: 18px;">Press save to continue</div>
            </div>
        `;
        
        // Initialize Teams configuration after rendering
        setTimeout(() => {
            initializeTeamsConfig();
        }, 0);
        
        return container;
    }
    
    function initializeTeamsConfig() {
        const baseUrl = window.location.origin;
        
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.pages.config.registerOnSaveHandler((saveEvent) => {
                microsoftTeams.pages.config.setConfig({
                    contentUrl: baseUrl + "/",
                    entityId: "DetailsTab",
                    suggestedDisplayName: "DetailsTab",
                    websiteUrl: baseUrl + "/",
                });
                saveEvent.notifySuccess();
            });
            microsoftTeams.pages.config.setValidityState(true);
        }).catch((error) => {
            console.error('Error initializing Teams config:', error);
        });
    }
    
    // Public API
    return {
        render: render
    };
})();
