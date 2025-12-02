const { stripMentionsText } = require("@microsoft/teams.api");
const { App } = require("@microsoft/teams.apps");
const { LocalStorage } = require("@microsoft/teams.common");
const config = require("./config");
const path = require('path');
const axios = require('axios');

// Load environment variables
const dotenv = require('dotenv');
const ENV_FILE = path.join(__dirname, 'env', '.env.local');
const ENV_USER_FILE = path.join(__dirname, 'env', '.env.local.user');
const LOCAL_CONFIGS = path.join(__dirname, '.localConfigs');
dotenv.config({ path: ENV_FILE });
dotenv.config({ path: ENV_USER_FILE });
dotenv.config({ path: LOCAL_CONFIGS }); // Load decrypted secrets

// Create storage for conversation history
const storage = new LocalStorage();

// Create the app with storage and credentials
const app = new App({
  clientId: process.env.CLIENT_ID || process.env.BOT_ID,
  clientSecret: process.env.CLIENT_SECRET || process.env.SECRET_BOT_PASSWORD,
  storage,
});

/**
 * Handle app installation events to send welcome message
 */
app.on("install.add", async (context) => {
  await context.send('Welcome to Typeahead search! This bot demonstrates adaptive card search capabilities.');
});

/**
 * Handle message events
 */
app.on("message", async (context) => {
  const activity = context.activity;
  const text = activity.text?.toLowerCase().trim();

  try {
    if (text === "staticsearch") {
      await context.send({
        type: 'message',
        attachments: [{
          contentType: "application/vnd.microsoft.card.adaptive",
          content: getStaticSearchCard()
        }]
      });
    } else if (text === "dynamicsearch") {
      await context.send({
        type: 'message',
        attachments: [{
          contentType: "application/vnd.microsoft.card.adaptive",
          content: getDynamicSearchCard()
        }]
      });
    } else if (text === "dependantdropdown") {
      const card = getDependantSearchCard();
      await context.send({
        type: 'message',
        attachments: [{
          contentType: "application/vnd.microsoft.card.adaptive",
          content: card
        }]
      });
    } else if (activity.value) {
      // Check if this is from the dependent dropdown (has both choiceselect and city)
      if (activity.value.city) {
        const country = activity.value.choiceselect;
        const cityValue = activity.value.city;
        
        const [cityCountry, cityName] = cityValue.split('-');
        
        // Validate that the selected city matches the selected country
        if (cityCountry === country) {
          await context.send(`You selected **${cityName}** from **${country.toUpperCase()}**`);
        } else {
          await context.send(`Error: You selected ${country.toUpperCase()} but chose a city from ${cityCountry.toUpperCase()}. Please select a matching city.`);
        }
      } else if (activity.value.choiceselect) {
        // This is from static search (IDE selection)
        const selectedIDE = activity.value.choiceselect;
        await context.send(`You selected IDE: **${selectedIDE.replace(/_/g, ' ').toUpperCase()}**`);
      } else {
        await context.send(`Received: ${JSON.stringify(activity.value)}`);
      }
    } else {
      await context.send("Hello! Use 'staticsearch', 'dynamicsearch', or 'dependantdropdown' to see typeahead search examples.");
    }
  } catch (error) {
    await context.send(`Error: ${error.message}`);
  }
});

/**
 * Handle static typeahead search
 */
app.on("search", async (context) => {
  const searchQuery = context.activity.value?.queryText?.toLowerCase() || '';
  const dataset = context.activity.value?.dataset;
  
  // Check if this is a city search (dataset === 'cities')
  if (dataset === 'cities') {
    const country = context.activity.value?.data?.choiceselect?.toLowerCase();
    
    if (!country) {
      return context.send({
        type: 'invokeResponse',
        value: {
          status: 200,
          body: {
            type: 'application/vnd.microsoft.search.searchResponse',
            value: { results: [] }
          }
        }
      });
    }
    
    const citiesByCountry = {
      'usa': [
        { title: "California", value: "California" },
        { title: "Florida", value: "Florida" },
        { title: "Texas", value: "Texas" },
        { title: "New York", value: "New York" },
        { title: "Illinois", value: "Illinois" }
      ],
      'france': [
        { title: "Paris", value: "Paris" },
        { title: "Lyon", value: "Lyon" },
        { title: "Nice", value: "Nice" },
        { title: "Marseille", value: "Marseille" },
        { title: "Toulouse", value: "Toulouse" }
      ],
      'india': [
        { title: "Delhi", value: "Delhi" },
        { title: "Mumbai", value: "Mumbai" },
        { title: "Pune", value: "Pune" },
        { title: "Bangalore", value: "Bangalore" },
        { title: "Chennai", value: "Chennai" }
      ]
    };

    const cities = citiesByCountry[country] || [];
    const filteredCities = searchQuery
      ? cities.filter(city => city.title.toLowerCase().includes(searchQuery))
      : cities;

    return context.send({
      type: 'invokeResponse',
      value: {
        status: 200,
        body: {
          type: 'application/vnd.microsoft.search.searchResponse',
          value: { results: filteredCities }
        }
      }
    });
  }
  
  // Static list of countries
  const countries = [
    { title: 'United States', value: 'US' },
    { title: 'United Kingdom', value: 'UK' },
    { title: 'Canada', value: 'CA' },
    { title: 'India', value: 'IN' },
    { title: 'Australia', value: 'AU' },
    { title: 'Germany', value: 'DE' },
    { title: 'France', value: 'FR' },
    { title: 'Japan', value: 'JP' },
    { title: 'China', value: 'CN' },
    { title: 'Brazil', value: 'BR' }
  ];

  // Filter based on search query
  const filteredCountries = searchQuery
    ? countries.filter(country => 
        country.title.toLowerCase().includes(searchQuery) ||
        country.value.toLowerCase().includes(searchQuery)
      )
    : countries;

  // Return results in the format Teams expects
  const results = filteredCountries.slice(0, 15).map(country => ({
    title: country.title,
    value: country.value
  }));

  await context.send({
    type: 'invokeResponse',
    value: {
      status: 200,
      body: {
        type: 'application/vnd.microsoft.search.searchResponse',
        value: {
          results: results
        }
      }
    }
  });
});

/**
 * Handle dynamic search (npm packages)
 */
app.on("search.npmpackages", async (context) => {
  const searchQuery = context.activity.value?.queryText || '';
  
  try {
    if (!searchQuery) {
      return context.send({
        type: 'invokeResponse',
        value: {
          status: 200,
          body: {
            type: 'application/vnd.microsoft.search.searchResponse',
            value: { results: [] }
          }
        }
      });
    }

    // Search npm registry
    const response = await axios.get(`https://registry.npmjs.org/-/v1/search`, {
      params: {
        text: searchQuery,
        size: 8
      }
    });

    const results = response.data.objects.map(pkg => ({
      title: pkg.package.name,
      value: `${pkg.package.name} - ${pkg.package.description || "No description available"}`
    }));

    await context.send({
      type: 'invokeResponse',
      value: {
        status: 200,
        body: {
          type: 'application/vnd.microsoft.search.searchResponse',
          value: { results }
        }
      }
    });
  } catch (error) {
    await context.send({
      type: 'invokeResponse',
      value: {
        status: 200,
        body: {
          type: 'application/vnd.microsoft.search.searchResponse',
          value: { results: [] }
        }
      }
    });
  }
});

/**
 * Handle dependent dropdown (cities based on country)
 */
app.on("search.cities", async (context) => {
  const searchQuery = context.activity.value?.queryText?.toLowerCase() || '';
  const country = context.activity.value?.data?.choiceselect?.toLowerCase();

  const citiesByCountry = {
    'usa': [
      { title: "CA", value: "CA" },
      { title: "FL", value: "FL" },
      { title: "TX", value: "TX" }
    ],
    'france': [
      { title: "Paris", value: "Paris" },
      { title: "Lyon", value: "Lyon" },
      { title: "Nice", value: "Nice" }
    ],
    'india': [
      { title: "Delhi", value: "Delhi" },
      { title: "Mumbai", value: "Mumbai" },
      { title: "Pune", value: "Pune" }
    ]
  };

  const cities = citiesByCountry[country] || [];
  const filteredCities = searchQuery
    ? cities.filter(city => city.title.toLowerCase().includes(searchQuery))
    : cities;

  await context.send({
    type: 'invokeResponse',
    value: {
      status: 200,
      body: {
        type: 'application/vnd.microsoft.search.searchResponse',
        value: { results: filteredCities }
      }
    }
  });
});

// Adaptive Card Templates
function getStaticSearchCard() {
  return {
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.2",
    "type": "AdaptiveCard",
    "body": [
      {
        "text": "Please search for the IDE from static list.",
        "wrap": true,
        "type": "TextBlock"
      },
      {
        "columns": [
          {
            "width": "auto",
            "items": [
              {
                "text": "IDE: ",
                "wrap": true,
                "height": "stretch",
                "type": "TextBlock"
              }
            ],
            "type": "Column"
          }
        ],
        "type": "ColumnSet"
      },
      {
        "columns": [
          {
            "width": "stretch",
            "items": [
              {
                "choices": [
                  { "title": "Visual studio", "value": "visual_studio" },
                  { "title": "IntelliJ IDEA", "value": "intelliJ_IDEA" },
                  { "title": "Aptana Studio 3", "value": "aptana_studio_3" },
                  { "title": "PyCharm", "value": "pycharm" },
                  { "title": "PhpStorm", "value": "phpstorm" },
                  { "title": "WebStorm", "value": "webstorm" },
                  { "title": "NetBeans", "value": "netbeans" },
                  { "title": "Eclipse", "value": "eclipse" },
                  { "title": "RubyMine", "value": "rubymine" },
                  { "title": "Visual studio code", "value": "visual_studio_code" }
                ],
                "style": "filtered",
                "placeholder": "Search for an IDE",
                "id": "choiceselect",
                "type": "Input.ChoiceSet"
              }
            ],
            "type": "Column"
          }
        ],
        "type": "ColumnSet"
      }
    ],
    "actions": [
      {
        "type": "Action.Submit",
        "id": "submit",
        "title": "Submit"
      }
    ]
  };
}

function getDynamicSearchCard() {
  return {
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.2",
    "type": "AdaptiveCard",
    "body": [
      {
        "text": "Please search for npm packages using dynamic search control.",
        "wrap": true,
        "type": "TextBlock"
      },
      {
        "columns": [
          {
            "width": "auto",
            "items": [
              {
                "text": "NPM packages search: ",
                "wrap": true,
                "height": "stretch",
                "type": "TextBlock"
              }
            ],
            "type": "Column"
          }
        ],
        "type": "ColumnSet"
      },
      {
        "columns": [
          {
            "width": "stretch",
            "items": [
              {
                "choices": [
                  { "title": "Static Option 1", "value": "static_option_1" },
                  { "title": "Static Option 2", "value": "static_option_2" },
                  { "title": "Static Option 3", "value": "static_option_3" }
                ],
                "isMultiSelect": false,
                "style": "filtered",
                "choices.data": {
                  "type": "Data.Query",
                  "dataset": "npmpackages"
                },
                "id": "choiceselect",
                "type": "Input.ChoiceSet"
              }
            ],
            "type": "Column"
          }
        ],
        "type": "ColumnSet"
      }
    ],
    "actions": [
      {
        "type": "Action.Submit",
        "id": "submitdynamic",
        "title": "Submit"
      }
    ]
  };
}

function getDependantSearchCard() {
  return {
    "type": "AdaptiveCard",
    "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.3",
    "body": [
      {
        "size": "ExtraLarge",
        "text": "Country and City Picker",
        "weight": "Bolder",
        "wrap": true,
        "type": "TextBlock"
      },
      {
        "type": "TextBlock",
        "text": "Select a country and then select the corresponding city from the list below.",
        "wrap": true,
        "spacing": "Small"
      },
      {
        "id": "choiceselect",
        "type": "Input.ChoiceSet",
        "label": "Country:",
        "choices": [
          { "title": "USA", "value": "usa" },
          { "title": "France", "value": "france" },
          { "title": "India", "value": "india" }
        ],
        "isRequired": true,
        "errorMessage": "Please select a country"
      },
      {
        "id": "city",
        "type": "Input.ChoiceSet",
        "label": "City/State (choose based on your selected country):",
        "style": "filtered",
        "choices": [
          { "title": "USA - CA", "value": "usa-CA" },
          { "title": "USA - FL", "value": "usa-FL" },
          { "title": "USA - TX", "value": "usa-TX" },
          { "title": "France - Paris", "value": "france-Paris" },
          { "title": "France - Lyon", "value": "france-Lyon" },
          { "title": "France - Nice", "value": "france-Nice" },
          { "title": "India - Delhi", "value": "india-Delhi" },
          { "title": "India - Mumbai", "value": "india-Mumbai" },
          { "title": "India - Pune", "value": "india-Pune" }
        ],
        "placeholder": "Type country name or city to filter",
        "isRequired": true,
        "errorMessage": "Please select a city"
      }
    ],
    "actions": [
      {
        "title": "Submit",
        "type": "Action.Submit"
      }
    ]
  };
}

module.exports = app;
