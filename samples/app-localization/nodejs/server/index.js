// Import required packages for path management, express server, and environment configuration
const path = require('path');
const express = require('express');

// Load environment variables from the .env file
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

// Initialize Express server
const server = express();

// Add middleware to parse JSON requests
server.use(express.json());

/**
 * Set up middleware to serve static files for the frontend (client build folder).
 * This will serve the bundled static files (HTML, JS, CSS) from the frontend build directory.
 */
server.use(express.static(path.resolve(__dirname, '../client/build')));

/**
 * API routes middleware.
 * Routes prefixed with '/api' are handled by the API module.
 */
server.use('/api', require('./api'));

/**
 * Fallback route for non-API requests.
 * Any requests not matched by the /api routes will fall back to serving the frontend's index.html.
 * This is necessary for Single Page Applications (SPAs) to handle client-side routing.
 */
server.get('*', (req, res) => {
    res.sendFile(path.resolve(__dirname, '../client/build', 'index.html'));
});

/**
 * Configure the server to listen on a port.
 * It will use the port from environment variables or default to 3978.
 */
const port = process.env.port || process.env.PORT || 3978;

/**
 * Start the Express server.
 * Log the URL where the server is running once the server is up and running.
 */
server.listen(port, () => {
    console.log(`\Bot/ME service listening at http://localhost:${port}`);
});
