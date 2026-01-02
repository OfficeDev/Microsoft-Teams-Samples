// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { createAgentApp } = require('../bot/botActivityHandler');

// Create the agent application instance
const agent = createAgentApp();

// Export the agent's request handler
const botHandler = async (req, res) => {
    // The agent handles routing internally via its middleware
    await agent.run(req, res);
};

// Export both the handler and the agent for use by express adapter
module.exports = { botHandler, agent };