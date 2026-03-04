// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.14.0

using MeetingApp.Bots;
using MeetingApp.Data.Repositories;
using MeetingApp.Data.Repositories.Feedback;
using MeetingApp.Data.Repositories.Notes;
using MeetingApp.Data.Repositories.Questions;
using MeetingApp.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;

namespace MeetingApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));

            services.AddHttpContextAccessor();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create a global hashset for our ConversationReferences
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            // Create a global hashset for our Roster and notes information
            services.AddSingleton<ConcurrentDictionary<string, ConversationData>>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, MeetingBot>();

            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddSingleton<ICandidateRepository>(new CandidateRepository(this.Configuration["StorageConnectionString"]));

            services.AddSingleton<IQuestionsRepository>(new QuestionsRepository(this.Configuration["StorageConnectionString"]));

            services.AddSingleton<INotesRepository>(new NotesRepository(this.Configuration["StorageConnectionString"]));

            services.AddSingleton<IFeedbackRepository>(new FeedbackRepository(this.Configuration["StorageConnectionString"]));

            // Storage we'll be using for User and Conversation state. 
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the Conversation state.  
            services.AddSingleton<ConversationState>();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseWebSockets();
            app.UseRouting();
            app.UseAuthorization();
            app.UseMvc();
            app.UseEndpoints(endpointRouteBuilder => endpointRouteBuilder.MapControllers());

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
