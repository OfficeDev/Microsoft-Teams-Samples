// <copyright file="UserLocationDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace AppCheckinLocation.Models
{
    /// <summary>
    /// User location detail model class.
    /// </summary>
    public class UserLocationDetail
    {
        public List<UserDetail> UserDetails { get; set; }
    }
}
