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

import { Request, Response } from "express";
import * as bodyParser from "body-parser";

module.exports.setup = function(app: any): void {
    let path = require("path");
    let express = require("express");

    // Configure the view engine, views folder and the statics path
    app.set("view engine", "pug");
    app.set("views", path.join(__dirname, "views"));

    app.use(bodyParser.json());
    app.use(bodyParser.urlencoded({
        extended: true,
    }));

    // Setup home page
    app.get("/", function(req: Request, res: Response): void {
        res.render("hello");
    });

    // Setup the static tab
    app.get("/hello", function(req: Request, res: Response): void {
        res.render("hello");
    });

    // Setup the configure tab, with first and second as content tabs
    app.get("/configure", function(req: Request, res: Response): void {
        res.render("configure");
    });

    app.get("/first", function(req: Request, res: Response): void {
        res.render("first");
    });

    app.get("/second", function(req: Request, res: Response): void {
        res.render("second");
    });

    app.get("/taskmodule", function(req: Request, res: Response): void {
        // Render the template, passing the appId so it's included in the rendered HTML
        res.render("taskmodule", { appId: process.env.MICROSOFT_APP_ID });
    });

    app.get("/youtube", function(req: Request, res: Response): void {
        res.render("youtube");
    });

    app.get("/powerapp", function(req: Request, res: Response): void {
        res.render("powerapp");
    });

    app.get("/customform", function(req: Request, res: Response): void {
        // Render the template, passing the appId so it's included in the rendered HTML
        res.render("customform", { appId: process.env.MICROSOFT_APP_ID });
    });

    app.post("/register", function(req: Request, res: Response): void {
        console.log(`Form body via HTTP POST:\nName: ${req.body.name}\nEmail: ${req.body.email}\nFavorite book: ${req.body.favoriteBook}\n`);
    });
};
