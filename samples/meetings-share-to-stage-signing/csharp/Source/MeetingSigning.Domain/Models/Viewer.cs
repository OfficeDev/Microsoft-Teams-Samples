// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Models
{
    public class Viewer
    {
        /// <summary>
        /// The ID of the Viewer
        /// </summary>
        /// <remarks>This will be created automatically by Entity Framework if not provided</remarks>

        public Guid Id { get; set; }

        /// <summary>
        /// Observer object that contains information like AzureAD User UserId
        /// </summary>
        public User Observer { get; set; }

        public Viewer DeepCopy()
        {
            Viewer other = (Viewer)this.MemberwiseClone();
            other.Observer = this.Observer.DeepCopy();
            return other;
        }
    }
}
