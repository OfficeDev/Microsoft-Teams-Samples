// <copyright file="IBackgroundTaskWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace AppCheckinLocation.BackgroundService
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides helper methods for task wrapper.
    /// </summary>
    public interface IBackgroundTaskWrapper
    {
        /// <summary>
        /// Method to add task to the task collection.
        /// </summary>
        /// <param name="task">represents one task.</param>
        public void Enqueue(Task task);
    }
}