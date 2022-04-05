// <copyright file="IncidentList.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace SequentialUserSpecificFlow.Models
{
    /// <summary>
    /// Incident list model class.
    /// </summary>
    public class IncidentList
    {
        public IncidentChoiceSet[] incidentList { get; set; }
    }

    /// <summary>
    /// Incident choice set model class.
    /// </summary>
    public class IncidentChoiceSet
    {
        public string title { get; set; }

        public Guid value { get; set; }
    }
}
