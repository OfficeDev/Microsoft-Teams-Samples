// <copyright file="UserDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace CheckInLocation.Models
{
    /// <summary>
    /// User Details model class.
    /// </summary>
    public class UserDetails
    {
        public string UserName { get; set; }

        public string CheckInTime { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string UserId { get; set; }
    }
}
