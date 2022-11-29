// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Client.Authentication;
using Microsoft.Graph.Communications.Common;

namespace CallingBotSample.Authentication
{
    public class AuthenticationWrapper : IRequestAuthenticationProvider, IAuthenticationProvider
    {
        private readonly IRequestAuthenticationProvider authenticationProvider;
        private readonly string tenant;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationWrapper"/> class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="tenant">The tenant.</param>
        public AuthenticationWrapper(IRequestAuthenticationProvider authenticationProvider, string? tenant = null)
        {
            this.authenticationProvider = authenticationProvider.NotNull(nameof(authenticationProvider));
            this.tenant = tenant;
        }

        /// <inheritdoc />
        public Task AuthenticateOutboundRequestAsync(HttpRequestMessage request, string tenant)
        {
            return this.authenticationProvider.AuthenticateOutboundRequestAsync(request, tenant);
        }

        /// <inheritdoc />
        public Task<RequestValidationResult> ValidateInboundRequestAsync(HttpRequestMessage request)
        {
            return this.authenticationProvider.ValidateInboundRequestAsync(request);
        }

        /// <inheritdoc />
        public Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            return this.AuthenticateOutboundRequestAsync(request, this.tenant);
        }
    }
}
