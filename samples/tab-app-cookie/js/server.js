// Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
// See LICENSE in the project root for license information.

var express = require("express");
var cookieParser = require("cookie-parser");
var serveStatic = require("serve-static");

var app = express();
app.use(cookieParser());
app.use(serveStatic(__dirname + "/public"));

app.get("/cookies/samesite/set", (req, res) => {
  res.cookie("3pcookie", "value", { sameSite: "none", secure: true });
  res.cookie("3pcookie-legacy", "value", { secure: true });
  res.cookie("3pcookie-insecure", "value", { sameSite: "none" });
  res.cookie("3pcookie-insecure-legacy", "value");
  res.end();
});

app.get("/cookies/partitioned/set", (req, res) => {
  res.setHeader("Set-Cookie", [
    "__Host-partitioned=DELETE FROM DEVTOOLS; SameSite=None; Secure; Path=/; Partitioned",
    "__Host-partitioned-lax=DELETE FROM DEVTOOLS; SameSite=Lax; Secure; Path=/; Partitioned",
    "__Host-partitioned-strict=DELETE FROM DEVTOOLS; SameSite=Strict; Secure; Path=/; Partitioned",
    "__Host-httpOnly-samesiteNone-partitioned=DELETE FROM DEVTOOLS; SameSite=None; Secure; HttpOnly; Path=/;",
    "__Host-not-partitioned=DELETE FROM DEVTOOLS; SameSite=None; Secure; Path=/",
    "__Host-no-partition-attribute=DELETE FROM DEVTOOLS; SameSite=None; Secure; Path=/;",
    "__Host-httponly-no-partition-attribute=DELETE FROM DEVTOOLS; SameSite=None; Secure; HttpOnly; Path=/;",
    "__Host-httpOnly-no-samesite-no-partition=DELETE FROM DEVTOOLS; Secure; HttpOnly; Priority=HIGH; Path=/;"
  ]);

  res.end();
});

var port = process.env.port || 3000;
app.listen(port, function () {
  console.log("Listening on http://localhost:" + port);
});
