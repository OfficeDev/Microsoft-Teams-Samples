// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MessageExtension_SP.Models
{
    /// <summary>
    /// InvokeResponseBody
    /// </summary>
    public class InvokeResponseBody
    {
        public int statusCode { get; set; }
        public string type { get; set; }
        public object value { get; set; }
    }
}
