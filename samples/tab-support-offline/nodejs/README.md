# SupportTickets
A classic CRUD app working in offline mode inside Microsoft Teams

## running locally
1. Start server ```nodemon index```, it starts the server on 8080
2. Start client ```npm run start```, client starts on port 3000, since proxy is defined as http://localhost:8080 in client's package.json, call REST API calls will be successful.

## deploying
1. build client ```npm run build```
2. build command aboves moves ```client/build``` folder to server directory
3. Deploy to Azure App Service