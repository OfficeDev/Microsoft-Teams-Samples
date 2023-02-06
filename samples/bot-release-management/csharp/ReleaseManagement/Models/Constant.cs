// <copyright file="Constant.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReleaseManagement.Models
{
    /// <summary>
    /// Constant value class.
    /// </summary>
    public static class Constant
    {
        // Key to get stakeholder team from incoming workitem payload.
        public static string StakeHolderTeamKey = "Custom.StakeholderTeam";

        // Key to get task title from incoming workitem payload.
        public static string TaskTitleKey = "System.Title";

        // Key to get created by user details from incoming workitem payload.
        public static string CreatedByKey = "System.CreatedBy";

        // Key to get assigned to user details from incoming workitem payload.
        public static string AssignedTo = "System.AssignedTo";

        // Key to get state of incoming workitem payload.
        public static string StateKey = "System.State";

        // Key to get task details from incoming workitem payload.
        public static string TaskDetails = "TaskDetails";
    }
}
