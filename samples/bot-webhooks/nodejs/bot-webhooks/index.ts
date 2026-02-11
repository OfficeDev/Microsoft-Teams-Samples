import { App } from '@microsoft/teams.apps';
import axios from 'axios';
import path from 'path';

let currentWebhookUrl = '';

const app = new App();

// Serve the tab UI (simple static HTML)
app.tab('configure-message', path.resolve(__dirname, './public'));

// Send card to webhook
app.http.post('/api/Send', async (req, res) => {
  try {
    const { webhookUrl, cardBody } = req.body;
    currentWebhookUrl = webhookUrl;
    
    await axios.post(webhookUrl, JSON.parse(cardBody));
    res.json({ success: true });
  } catch (error: any) {
    res.status(500).json({ error: error.message });
  }
});

// Handle card action callback
app.http.post('/api/save', async (req, res) => {
  try {
    if (currentWebhookUrl) {
      await axios.post(currentWebhookUrl, {
        type: 'message',
        attachments: [{
          contentType: 'application/vnd.microsoft.card.adaptive',
          content: {
            type: 'AdaptiveCard',
            version: '1.5',
            body: [{ type: 'TextBlock', text: `Response: ${JSON.stringify(req.body)}`, wrap: true }]
          }
        }]
      });
    }
    res.json({ success: true });
  } catch (error: any) {
    res.status(500).json({ error: error.message });
  }
});

app.start().then(() => {
  console.log('Server: http://localhost:3978');
  console.log('Tab: http://localhost:3978/tabs/configure-message');
});