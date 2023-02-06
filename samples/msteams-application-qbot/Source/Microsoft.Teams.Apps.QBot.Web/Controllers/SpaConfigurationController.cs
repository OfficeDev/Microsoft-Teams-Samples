// <copyright file="SpaConfigurationController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Web.Bot
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.QBot.Web.SpaHost;

    /// <summary>
    /// The SpaConfigurationController is responsible for serving the site-specific config for the qbot single-page application
    /// </summary>
    /// <remarks>
    /// This is not nested under the "api" since this is coupled with the single-page-app and not the api surface.
    ///
    /// TODO(nibeauli): Add ETag and caching capabilities to this API.
    /// </remarks>
    [Route("/app/config")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public sealed class SpaConfigurationController : ControllerBase
    {
        private readonly SpaHostConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpaConfigurationController"/> class.
        /// </summary>
        /// <param name="options">The single-page-app options </param>
        public SpaConfigurationController(IOptions<SpaHostConfiguration> options)
        {
            this.configuration = options.Value;
        }

        /// <summary>
        /// Get the configuration for the QBot single-page-app.
        /// </summary>
        /// <returns>The QBot SPA configuration</returns>
        [HttpGet]
        public ActionResult<SpaHostConfiguration> Get()
        {
            return new OkObjectResult(this.configuration);
        }
    }
}