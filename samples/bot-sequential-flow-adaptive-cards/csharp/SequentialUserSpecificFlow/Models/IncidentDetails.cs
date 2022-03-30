// <copyright file="IncidentDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;

namespace SequentialUserSpecificFlow.Models
{
    /// <summary>
    /// Incident details model class.
    /// </summary>
    public class IncidentDetails
    {
        public Guid IncidentId { get; set; }

        public string CreatedBy { get; set; }

        public string IncidentTitle { get; set; }

        public string AssignedToMRI { get; set; }

        public string Category { get; set; }

        public string SubCategory { get; set; }

        public string AssignedToName { get; set; }
    }
}
