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

// Activity types
export const messageType = "message";
export const invokeType = "invoke";

// Dialog ids
// tslint:disable-next-line:variable-name
export const DialogId = {
    Root: "/",
    ACTester: "actester",
    BFCard: "bfcard",
};

// Telemetry events
// tslint:disable-next-line:variable-name
export const TelemetryEvent = {
    UserActivity: "UserActivity",
    BotActivity: "BotActivity",
};

// URL Placeholders - not currently supported
// tslint:disable-next-line:variable-name
export const UrlPlaceholders = "loginHint={loginHint}&upn={userPrincipalName}&aadId={userObjectId}&theme={theme}&groupId={groupId}&tenantId={tid}&locale={locale}";

// Task Module Strings
// tslint:disable-next-line:variable-name
export const TaskModuleStrings = {
    YouTubeTitle: "Microsoft Ignite 2018 Vision Keynote",
    PowerAppTitle: "PowerApp: Asset Checkout",
    CustomFormTitle: "Custom Form",
    AdaptiveCardTitle: "Create a new job posting",
    AdaptiveCardKitchenSinkTitle: "Adaptive Card: Inputs",
    ActionSubmitResponseTitle: "Action.Submit Response",
    YouTubeName: "YouTube",
    PowerAppName: "PowerApp",
    CustomFormName: "Custom Form",
    AdaptiveCardSingleName: "Adaptive Card - Single",
    AdaptiveCardSequenceName: "Adaptive Card - Sequence",
};

// Task Module Ids
// tslint:disable-next-line:variable-name
export const TaskModuleIds = {
    YouTube: "youtube",
    PowerApp: "powerapp",
    CustomForm: "customform",
    AdaptiveCard1: "adaptivecard1",
    AdaptiveCard2: "adaptivecard2",
};

// Task Module Sizes
// tslint:disable-next-line:variable-name
export const TaskModuleSizes = {
    youtube: {
        width: 1000,
        height: 700,
    },
    powerapp: {
        width: 720,
        height: 520,
    },
    customform: {
        width: 510,
        height: 430,
    },
    adaptivecard: {
        width: 700,
        height: 255,
    },
};
