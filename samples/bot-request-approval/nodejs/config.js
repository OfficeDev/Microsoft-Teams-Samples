/**
 * Configuration for Request Approval Bot
 * Enhanced configuration with validation for Teams AI Library SDK V2
 */

// Validate required environment variables
const requiredEnvVars = ['CLIENT_ID'];
const missingVars = requiredEnvVars.filter(varName => !process.env[varName]);

if (missingVars.length > 0) {
  process.exit(1);
}

const config = {
  // Bot Framework Authentication
  MicrosoftAppId: process.env.CLIENT_ID,
  MicrosoftAppType: process.env.BOT_TYPE || 'MultiTenant',
  MicrosoftAppTenantId: process.env.TENANT_ID,
  MicrosoftAppPassword: process.env.CLIENT_PASSWORD,
  
  // Bot Configuration
  Port: process.env.PORT || process.env.port || 3978,
  
  // Request Approval Specific Settings
  MaxTaskTitleLength: 100,
  MaxTaskDescriptionLength: 500,
  TaskRetentionDays: 30,
  
  // Teams AI Library SDK V2 Settings
  TeamsAIVersion: '2.0.0',
  
  // Feature Flags
  Features: {
    TaskManagement: true,
    AdaptiveCards: true,
    UserRefresh: true,
    TaskEditing: true,
    TaskCancellation: true
  },
  
  // Logging Configuration
  LogLevel: process.env.LOG_LEVEL || 'info',
  
  // Validate the configuration
  validate() {
    const errors = [];
    
    if (!this.MicrosoftAppId) {
      errors.push('MicrosoftAppId (CLIENT_ID) is required');
    }
    
    if (this.MicrosoftAppType && !['MultiTenant', 'SingleTenant', 'UserAssignedMsi'].includes(this.MicrosoftAppType)) {
      errors.push('Invalid MicrosoftAppType. Must be MultiTenant, SingleTenant, or UserAssignedMsi');
    }
    
    if (errors.length > 0) {
      errors.forEach(error => console.error(`  - ${error}`));
      return false;
    }
    return true;
  }
};

// Validate configuration on load
if (!config.validate()) {
  process.exit(1);
}

module.exports = config;
