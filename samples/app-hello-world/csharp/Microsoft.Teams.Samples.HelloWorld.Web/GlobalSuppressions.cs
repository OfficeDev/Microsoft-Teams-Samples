// <copyright file="GlobalSuppressions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "This code needs to be uncommented for local debugging", Scope = "member", Target = "~M:Microsoft.Teams.Samples.HelloWorld.Web.AdapterWithErrorHandler.#ctor(Microsoft.Extensions.Configuration.IConfiguration,Microsoft.Extensions.Logging.ILogger{Microsoft.Bot.Builder.Integration.AspNet.Core.BotFrameworkHttpAdapter})")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "This URL will not change later", Scope = "member", Target = "~M:Microsoft.Teams.Samples.HelloWorld.Web.MessageExtension.GetAttachment(System.String)~Microsoft.Bot.Schema.Teams.MessagingExtensionAttachment")]
