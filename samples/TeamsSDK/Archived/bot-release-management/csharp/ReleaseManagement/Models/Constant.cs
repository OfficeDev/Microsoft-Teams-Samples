// <copyright file="Constant.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Models
{
    /// <summary>
    /// Constant values.
    /// </summary>
    public static class Constant
    {
        /// <summary>
        /// Key to get stakeholder team from incoming workitem payload.
        /// </summary>
        public const string StakeHolderTeamKey = "Custom.StakeholderTeams";

        /// <summary>
        /// Key to get task title from incoming workitem payload.
        /// </summary>
        public const string TaskTitleKey = "System.Title";

        /// <summary>
        /// Key to get created by user details from incoming workitem payload.
        /// </summary>
        public const string CreatedByKey = "System.CreatedBy";

        /// <summary>
        /// Key to get assigned to user details from incoming workitem payload.
        /// </summary>
        public const string AssignedTo = "System.AssignedTo";

        /// <summary>
        /// Key to get state of incoming workitem payload.
        /// </summary>
        public const string StateKey = "System.State";

        /// <summary>
        /// Key to store task details in cache.
        /// </summary>
        public const string TaskDetails = "TaskDetails";
    }
}
