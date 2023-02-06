// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories.InMemory
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <summary>
    /// ViewerRepository is an inmemory implementation of IViewerRepository
    /// </summary>
    public class ViewerRepository : IViewerRepository
    {
        private readonly IDictionary<Guid, Viewer> viewerDictionary;

        public ViewerRepository()
        {
            this.viewerDictionary = new Dictionary<Guid, Viewer>();
        }

        /// <summary>
        /// AddViewer tries to insert a new viewer who is an existing user as well.
        /// </summary>
        /// <param name="viewer"></param>
        /// <returns>Viewer added or null</returns>
        public async Task<Viewer> AddViewer(Viewer viewer)
        {
            if (this.viewerDictionary.TryGetValue(viewer.Id, out Viewer existingViewer))
            {
                return null;
            }
            viewer.Id = Guid.NewGuid();
            this.viewerDictionary.Add(viewer.Id, viewer);
            return viewer;
        }
    }
}
