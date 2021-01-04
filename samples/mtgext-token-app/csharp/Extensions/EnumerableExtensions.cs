// <copyright file="EnumerableExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// EnumerableExtensions extensions to act on enumerable.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// IsNullOrEmpty checks if the enumberable is null or empty.
        /// </summary>
        /// <typeparam name="T">The element type of the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable on which the check has to be performed.</param>
        /// <returns>A <see cref="bool"/> return true if the enumerable is empty.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable?.Any() ?? true;
        }
    }
}
