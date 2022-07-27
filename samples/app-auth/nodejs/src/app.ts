// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

let express = require("express");
let exphbs = require("express-handlebars");
import { Request, Response } from "express";
let bodyParser = require("body-parser");
let favicon = require("serve-favicon");
let http = require("http");
let path = require("path");
import * as builder from "botbuilder";
import * as config from "config";
import * as apis from "./apis";
import { AuthBot } from "./AuthBot";
import { logger } from "./utils/index";
import { RootDialog } from "./dialogs/RootDialog";
import { AzureADDialog } from "./dialogs/AzureADDialog";
import { LinkedInDialog } from "./dialogs/LinkedInDialog";
import { GoogleDialog } from "./dialogs/GoogleDialog";

let app = express();
let appId = config.get("app.appId");

app.set("port", process.env.PORT || 3978);
app.use(express.static(path.join(__dirname, "../public")));
app.use(favicon(path.join(__dirname, "../public/assets", "favicon.ico")));
app.use(bodyParser.json());

let handlebars = exphbs.create({
    extname: ".hbs",
    helpers: {
        appId: () => { return appId; },
    },
    defaultLayout: false,
});
app.engine("hbs", handlebars.engine);
app.set("view engine", "hbs");

// Configure storage
const botStorage = new builder.MemoryStorage();
const conversationState = new builder.ConversationState(botStorage);
const userState = new builder.UserState(botStorage);

// Create adapter
const adapter = new builder.BotFrameworkAdapter({
    appId: config.get("bot.appId"),
    appPassword: config.get("bot.appPassword"),
});

// Create dialogs and bot
const identityProviderDialogs = [];

function addDialog<TDialog>(TCreator: { new (connectionName: string): TDialog}, configurationName: string) {
    if (config.has(configurationName) && config.get(configurationName)) {
        identityProviderDialogs.push(new TCreator(config.get(configurationName)));
    }
}
addDialog(AzureADDialog, "azureAD.connectionName");
addDialog(LinkedInDialog, "linkedIn.connectionName");
addDialog(GoogleDialog, "google.connectionName");

let bot = new AuthBot(adapter, conversationState, userState, new RootDialog(identityProviderDialogs), identityProviderDialogs);

// Configure bot routes
app.post("/api/messages", (req, res) => {
    adapter.processActivity(req, res, async (turnContext) => {
        // Route the message to the bot's main handler.
        await bot.run(turnContext);
    });
});

// Tab authentication sample routes
app.get("/tab/simple", (req, res) => { res.render("tab/simple/simple"); });
app.get("/tab/simple-start", (req, res) => { res.render("tab/simple/simple-start"); });
app.get("/tab/simple-start-v2", (req, res) => { res.render("tab/simple/simple-start-v2"); });
app.get("/tab/simple-end", (req, res) => { res.render("tab/simple/simple-end"); });
app.get("/tab/silent", (req, res) => { res.render("tab/silent/silent"); });
app.get("/tab/silent-start", (req, res) => { res.render("tab/silent/silent-start"); });
app.get("/tab/silent-end", (req, res) => { res.render("tab/silent/silent-end"); });
app.get("/tab/sso", (req, res) => { res.render("tab/sso/sso"); });

let openIdMetadataV1 = new apis.OpenIdMetadata("https://login.microsoftonline.com/common/.well-known/openid-configuration");
let openIdMetadataV2 = new apis.OpenIdMetadata("https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration");
let validateAzureADToken = new apis.ValidateAzureADToken(openIdMetadataV1, openIdMetadataV2, appId).listen();     // Middleware to validate id_token

app.get("/api/decodeToken", validateAzureADToken, new apis.DecodeToken().listen());
app.get("/api/getProfileFromGraph", validateAzureADToken, new apis.GetProfileFromGraph(config.get("app.appId"), config.get("bot.appPassword")).listen());
app.get("/api/getProfilesFromBot", validateAzureADToken, async (req, res) => {
    let profiles = await bot.getUserProfilesAsync(res.locals.token["oid"]);
    res.status(200).send(profiles);
});

// Configure ping route
app.get("/ping", (req, res) => {
    res.status(200).send("OK");
});

// error handlers

// development error handler
// will print stacktrace
if (app.get("env") === "development") {
    app.use(function(err: any, req: Request, res: Response, next: Function): void {
        logger.error("Failed request", err);
        res.status(err.status || 500).send(err);
    });
}

// production error handler
// no stacktraces leaked to user
app.use(function (err: any, req: Request, res: Response, next: Function): void {
    logger.error("Failed request", err);
    res.sendStatus(err.status || 500);
});

http.createServer(app).listen(3978, function (): void {
    console.log("Serve is running!!!");
    logger.verbose("Express server listening on port " + app.get("port"));
    logger.verbose("Bot messaging endpoint: " + config.get("app.baseUri") + "/api/messages");
});
