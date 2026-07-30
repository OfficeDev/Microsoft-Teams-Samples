// <copyright file="UserDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace AppCheckinLocation.Models
{
    /// <summary>
    /// User Detail model class.
    /// </summary>
    public class UserDetail
    {
        public string UserName { get; set; }

        public string CheckInTime { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string UserId { get; set; }
    }
}
