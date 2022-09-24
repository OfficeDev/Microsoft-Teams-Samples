// <copyright file="KnowledgeBasesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.QBot.Domain.KnowledgeBases;
    using Microsoft.Teams.Apps.QBot.Domain.Models;
    using Microsoft.Teams.Apps.QBot.Web.Authorization;

    /// <summary>
    /// <see cref="KnowledgeBase"/> APIs.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(ErrorResponseFilterAttribute))]
    public class KnowledgeBasesController : ControllerBase
    {
        private readonly IKnowledgeBaseReader kbReader;
        private readonly IKnowledgeBaseWriter kbWriter;
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBasesController"/> class.
        /// </summary>
        /// <param name="kbReader">The knowledge base reader.</param>
        /// <param name="kbWriter">The knowledge base writer.</param>
        /// <param name="authorizationService">AuthZ service.</param>
        public KnowledgeBasesController(
            IKnowledgeBaseReader kbReader,
            IKnowledgeBaseWriter kbWriter,
            IAuthorizationService authorizationService)
        {
            this.kbReader = kbReader ?? throw new ArgumentNullException(nameof(kbReader));
            this.kbWriter = kbWriter ?? throw new ArgumentNullException(nameof(kbWriter));
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        /// <summary>
        /// GET api/knowledgebases
        ///
        /// Gets all the knowledgebases.
        /// </summary>
        /// <returns>List of <see cref="KnowledgeBase"/>.</returns>
        [HttpGet]
        [Authorize(Policy = AuthZPolicy.AdminPolicy)]
        public async Task<ActionResult<IEnumerable<KnowledgeBase>>> GetAllAsync()
        {
            var kbs = await this.kbReader.GetAllKnowledgeBasesAsync();
            return new OkObjectResult(kbs);
        }

        /// <summary>
        /// GET api/knowledgebases/{id}
        ///
        /// Gets the knowledge base.
        /// </summary>
        /// <param name="id">KnowledgeBase's id.</param>
        /// <returns><see cref="KnowledgeBase"/>.</returns>
        [HttpGet("{id}", Name = "GetKnowledgeBase")]
        public async Task<ActionResult<KnowledgeBase>> GetKnowledgeBaseAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new BadRequestResult();
            }

            // Authorize
            var kb = await this.kbReader.GetKnowledgeBaseAsync(id);
            await this.authorizationService.AuthorizeAsync(this.User, kb.OwnerUserId, AuthZPolicy.UserResourcePolicy);

            return new OkObjectResult(kb);
        }

        /// <summary>
        /// POST api/knowledgebases
        ///
        /// Creates a new knowledge base.
        /// </summary>
        /// <param name="knowledgeBase">Knowledge base.</param>
        /// <returns><see cref="KnowledgeBase"/>.</returns>
        [HttpPost]
        public async Task<ActionResult<KnowledgeBase>> PostAsync([FromBody] KnowledgeBase knowledgeBase)
        {
            knowledgeBase = await this.kbWriter.AddKnowledgeBaseAsync(knowledgeBase);
            return new CreatedAtRouteResult(
                routeName: "GetKnowledgeBase",
                routeValues: new
                {
                    knowledgeBase.Id,
                },
                value: knowledgeBase);
        }

        /// <summary>
        /// PUT api/knowledgebases/{id}
        ///
        /// Updates an existing knowledge base.
        /// </summary>
        /// <param name="id">Knowledge base's id.</param>
        /// <param name="knowledgeBase">Updated knowledge base.</param>
        /// <returns><see cref="KnowledgeBase"/>.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(string id, [FromBody] KnowledgeBase knowledgeBase)
        {
            if (string.IsNullOrEmpty(id) || knowledgeBase == null || knowledgeBase.Id != id)
            {
                return new BadRequestResult();
            }

            // Authorize
            var kb = await this.kbReader.GetKnowledgeBaseAsync(id);
            await this.authorizationService.AuthorizeAsync(this.User, kb.OwnerUserId, AuthZPolicy.UserResourcePolicy);

            await this.kbWriter.UpdateKnowledgeBaseAsync(knowledgeBase);
            return await Task.FromResult(this.StatusCode(204));
        }

        /// <summary>
        /// DELETE api/knowledgebases/{id}
        ///
        /// Deletes an existing knowledge base.
        /// </summary>
        /// <param name="id">Knowledge base's id.</param>
        /// <returns><see cref="KnowledgeBase"/>.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new BadRequestResult();
            }

            // Authorize
            var kb = await this.kbReader.GetKnowledgeBaseAsync(id);
            await this.authorizationService.AuthorizeAsync(this.User, kb.OwnerUserId, AuthZPolicy.UserResourcePolicy);

            await this.kbWriter.DeleteKnowledgeBaseAsync(id);
            return await Task.FromResult(this.StatusCode(204));
        }
    }
}
