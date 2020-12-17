// <copyright file="StringExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Extensions
{
    /// <summary>
    /// String extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Check if the string reference is not null, whitespace or empty.
        /// </summary>
        /// <param name="value">the string reference.</param>
        /// <returns>value indicating if string is not null,whitespace or empty.</returns>
        public static bool IsValid(this string value) => !string.IsNullOrWhiteSpace(value)
            && !string.IsNullOrEmpty(value);
    }
}
