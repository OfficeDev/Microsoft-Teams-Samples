// <copyright file="InitialSequentialCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TypeaheadSearch.Models
{
    /// <summary>
    /// Static search Card model class.
    /// </summary>
    public class StaticSearchCard
    {
        public string choiceSelect { get; set; }
    }

    /// <summary>
    /// Dynamic search Card model class.
    /// </summary>
    public class DynamicSearchCard
    {
        public string queryText { get; set; }
    }
}
