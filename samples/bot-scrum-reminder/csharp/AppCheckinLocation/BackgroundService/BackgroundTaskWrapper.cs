// <copyright file="BackgroundTaskWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace AppCheckinLocation.BackgroundService
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    /// <summary>
    /// Wrapper class with properties and methods to manage Tasks.
    /// </summary>
    public class BackgroundTaskWrapper : IBackgroundTaskWrapper, IDisposable
    {
        /// <summary>
        /// Thread safe collection of tasks.
        /// </summary>
        private readonly BlockingCollection<Task> taskCollection;

        private bool disposedValue; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskWrapper"/> class.
        /// </summary>
        public BackgroundTaskWrapper() => this.taskCollection = new BlockingCollection<Task>();

        /// <summary>
        /// Method to add task to the task collection.
        /// </summary>
        /// <param name="task">represents one task.</param>
        public void Enqueue(Task task) => this.taskCollection.Add(task);

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        /// <param name="disposing">A boolean value to determine if a resource is to be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.taskCollection.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}
