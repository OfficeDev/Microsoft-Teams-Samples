// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable disable

namespace AzureSamples.Security.KeyVault.Proxy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Core;

    /// <summary>
    /// A cached <see cref="Response"/> that is cloned and returned for subsequent requests.
    ///
    /// Source: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/keyvault/samples/keyvaultproxy/src
    /// </summary>
    internal class CachedResponse : Response
    {
        private readonly ResponseHeaders headers;
        private DateTimeOffset expires;

        private CachedResponse(int status, string reasonPhrase, ResponseHeaders headers)
        {
            this.Status = status;
            this.ReasonPhrase = reasonPhrase;

            this.headers = headers;
        }

        /// <inheritdoc/>
        public override int Status { get; }

        /// <inheritdoc/>
        public override string ReasonPhrase { get; }

        /// <inheritdoc/>
        public override Stream ContentStream { get; set; }

        /// <inheritdoc/>
        public override string ClientRequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CachedResponse"/> is still valid (has not expired).
        /// </summary>
        internal bool IsValid => DateTimeOffset.Now <= this.expires;

        /// <inheritdoc/>
        public override void Dispose() => this.ContentStream?.Dispose();

        /// <summary>
        /// Creates a new <see cref="CachedResponse"/>.
        /// </summary>
        /// <param name="isAsync">Whether to copy the <see cref="ContentStream"/> asynchronously.</param>
        /// <param name="response">The <see cref="Response"/> to copy.</param>
        /// <param name="ttl">The time to live.</param>
        /// <returns>A <see cref="CachedResponse"/> copied from the <paramref name="response"/>.</returns>
        internal static async ValueTask<CachedResponse> CreateAsync(bool isAsync, Response response, TimeSpan ttl)
        {
            CachedResponse cachedResponse = await CloneAsync(isAsync, response).ConfigureAwait(false);
            cachedResponse.expires = DateTimeOffset.Now + ttl;

            return cachedResponse;
        }

        /// <summary>
        /// Clones this <see cref="CachedResponse"/> into a new <see cref="Response"/>.
        /// </summary>
        /// <param name="isAsync">Whether to copy the <see cref="ContentStream"/> asynchronously.</param>
        /// <returns>A cloned <see cref="Response"/>.</returns>
        internal async ValueTask<Response> CloneAsync(bool isAsync) =>
            await CloneAsync(isAsync, this).ConfigureAwait(false);

        /// <inheritdoc/>
        protected override bool ContainsHeader(string name) => this.headers.Contains(name);

        /// <inheritdoc/>
        protected override IEnumerable<HttpHeader> EnumerateHeaders() => this.headers;

        /// <inheritdoc/>
        protected override bool TryGetHeader(string name, out string value) => this.headers.TryGetValue(name, out value);

        /// <inheritdoc/>
        protected override bool TryGetHeaderValues(string name, out IEnumerable<string> values) => this.headers.TryGetValues(name, out values);

        private static async ValueTask<CachedResponse> CloneAsync(bool isAsync, Response response)
        {
            CachedResponse cachedResponse = new CachedResponse(response.Status, response.ReasonPhrase, response.Headers)
            {
                ClientRequestId = response.ClientRequestId,
            };

            if (response.ContentStream is { })
            {
                MemoryStream ms = new MemoryStream();
                cachedResponse.ContentStream = ms;

                if (isAsync)
                {
                    await response.ContentStream.CopyToAsync(cachedResponse.ContentStream).ConfigureAwait(false);
                }
                else
                {
                    response.ContentStream.CopyTo(cachedResponse.ContentStream);
                }

                ms.Position = 0;

                // Reset the position if we can; otherwise, copy the buffer.
                if (response.ContentStream.CanSeek)
                {
                    response.ContentStream.Position = 0;
                }
                else
                {
                    response.ContentStream = new MemoryStream(ms.ToArray());
                }
            }

            return cachedResponse;
        }
    }
}