// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MeetingExtension_SP.Models.Sharepoint
{
    /// <summary>
    /// Wrapper for SP
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SPWrapper<T> : SharePointBase
    {
        public T[] Value { get; set; }
    }
}
