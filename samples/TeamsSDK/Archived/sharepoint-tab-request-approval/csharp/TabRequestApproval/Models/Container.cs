// <copyright file="Container.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Models
{
    /// <summary>
    /// Represents the model for the container.
    /// The fields start with a lowercase.
    /// That is the way the API requires them.
    /// </summary>
    public class Container
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="id">Represents the id.</param>
        /// <param name="displayName">Represents the display name.</param>
        /// <param name="description">Represents the description.</param>
        /// <param name="containerTypeId">Represents the container type id.</param>
        /// <param name="status">Represents the status.</param>
        public Container(string id = null, string displayName = null, string description = null, string containerTypeId = null, string status = null)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
            this.containerTypeId = containerTypeId;
            this.status = status;
        }

        /// <summary>
        /// Gets or sets represents the id.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Gets or sets represents the display name.
        /// </summary>
        public string displayName { get; set; }

        /// <summary>
        /// Gets or sets represents the description.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Gets or sets represents the containerTypeId.
        /// </summary>
        public string containerTypeId { get; set; }

        /// <summary>
        /// Gets or sets represents the date time the container was created.
        /// </summary>
        public string createdDateTime { get; set; }

        /// <summary>
        /// Gets or sets represents the status of the container.
        /// </summary>
        public string status { get; set; }
    }
}
