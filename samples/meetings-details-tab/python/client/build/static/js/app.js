/**
 * Main App Router - JavaScript equivalent of React App.js
 * Handles client-side routing and component rendering
 */

window.App = (function() {
    'use strict';
    
    // Current route state
    let currentRoute = '';
    
    // Initialize the application
    function init() {
        // Set up router
        setupRouter();
        
        // Handle initial route
        handleRoute();
        
        // Listen for route changes
        window.addEventListener('popstate', handleRoute);
    }
    
    function setupRouter() {
        // Override link clicks to use client-side routing
        document.addEventListener('click', function(e) {
            if (e.target.tagName === 'A' && e.target.href.startsWith(window.location.origin)) {
                e.preventDefault();
                const path = e.target.pathname;
                history.pushState(null, '', path);
                handleRoute();
            }
        });
    }
    
    function handleRoute() {
        const path = window.location.pathname;
        
        // Map routes to components (matches React Router routes)
        switch (path) {
            case '/':
                renderComponent('welcome');
                break;
            case '/configuretab':
                renderComponent('config');
                break;
            case '/detail':
                renderComponent('detail');
                break;
            case '/result':
                renderComponent('result');
                break;
            default:
                // Default to welcome for unknown routes
                renderComponent('welcome');
                break;
        }
        
        currentRoute = path;
    }
    
    function renderComponent(componentName) {
        const root = document.getElementById('root');
        if (!root) {
            console.error('Root element not found');
            return;
        }
        
        // Clear current content
        root.innerHTML = '';
        
        // Render the appropriate component
        switch (componentName) {
            case 'welcome':
                if (window.Welcome && window.Welcome.render) {
                    root.appendChild(window.Welcome.render());
                } else {
                    root.innerHTML = '<div class="container mt-4"><h1>Loading Welcome...</h1></div>';
                }
                break;
                
            case 'config':
                if (window.Config && window.Config.render) {
                    root.appendChild(window.Config.render());
                } else {
                    root.innerHTML = '<div class="container mt-4"><h1>Loading Config...</h1></div>';
                }
                break;
                
            case 'detail':
                if (window.Detail && window.Detail.render) {
                    root.appendChild(window.Detail.render());
                } else {
                    root.innerHTML = '<div class="container mt-4"><h1>Loading Detail...</h1></div>';
                }
                break;
                
            case 'result':
                if (window.Result && window.Result.render) {
                    root.appendChild(window.Result.render());
                } else {
                    root.innerHTML = '<div class="container mt-4"><h1>Loading Result...</h1></div>';
                }
                break;
                
            default:
                root.innerHTML = '<div class="container mt-4"><h1>Page not found</h1></div>';
                break;
        }
    }
    
    function navigate(path) {
        history.pushState(null, '', path);
        handleRoute();
    }
    
    // Public API
    return {
        init: init,
        navigate: navigate,
        getCurrentRoute: function() { return currentRoute; }
    };
})();
