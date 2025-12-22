// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your bot

// Import required packages
const path = require('path');
const express = require('express');

// Read environment variables from local .env file
const ENV_FILE = path.join(__dirname, 'env', '.env.local');
require('dotenv').config({ path: ENV_FILE });

// TEAMS AI v2 ONLY SOLUTION: Let Teams AI v2 handle everything, add task module route
(async () => {
    try {
        // Starting Teams AI v2 with task module integration
        
        // Import Teams AI app
        const teamsApp = require('./app');
        
        // Hook into Teams AI v2 startup to add our task module route
        const originalStart = teamsApp.start;
        
        teamsApp.start = async function(...args) {
            // Starting Teams AI v2 server
            
            // Call original Teams AI v2 start method
            const result = await originalStart.apply(this, args);
            
            // After Teams AI starts, try to add our task module routes
            setTimeout(() => {
                try {
                    // Try accessing Teams AI's Express app through http.express
                    
                    let foundApp = null;
                    
                    if (this.http && this.http.express) {
                        foundApp = this.http.express;
                    } else if (this.http && this.http._server) {
                        foundApp = this.http._server;
                    } else {
                        
                        // Try other possible locations
                        const possibleApps = [
                            this._app,
                            this.app,
                            this._server?.app,
                            this.server?.app,
                            this.http?._app,
                            this.http?.app
                        ];
                        
                        for (const app of possibleApps) {
                            if (app && typeof app.get === 'function') {
                                foundApp = app;
                                break;
                            }
                        }
                    }
                    
                    if (foundApp) {
                        
                        // Configure EJS templating
                        foundApp.engine('html', require('ejs').renderFile);
                        foundApp.set('view engine', 'ejs');
                        foundApp.set('views', __dirname);
                        foundApp.use("/Images", express.static(path.resolve(__dirname, 'Images')));
                        
                        // Add task module route
                        foundApp.get('/scheduleTask', (req, res) => {
                            try {
                                res.render('./views/ScheduleTask');
                            } catch (renderError) {
                                res.status(500).send('Task module error');
                            }
                        });
                        
                        // Add health check
                        foundApp.get('/health', (req, res) => {
                            res.json({ 
                                status: 'Teams AI v2 with task module', 
                                integration: 'success',
                                taskModule: 'available',
                                timestamp: new Date().toISOString()
                            });
                        });
                        
                        // Task module successfully integrated
                        
                    } else {
                        // Fallback: Create separate task server on different port
                        const taskApp = express();
                        taskApp.engine('html', require('ejs').renderFile);
                        taskApp.set('view engine', 'ejs');
                        taskApp.set('views', __dirname);
                        taskApp.use("/Images", express.static(path.resolve(__dirname, 'Images')));
                        
                        taskApp.get('/scheduleTask', (req, res) => {
                            res.render('./views/ScheduleTask');
                        });
                        
                        taskApp.listen(8080, () => {
                            // Fallback task server running on port 8080
                        });
                    }
                    
                } catch (error) {
                    // Error integrating task module - continue silently
                }
            }, 1000); // Wait 1 second for Teams AI to fully initialize
            
            return result;
        };
        
        // Start Teams AI v2 with our integration
        await teamsApp.start();
        
    } catch (error) {
        process.exit(1);
    }
})();