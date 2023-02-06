// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories
{
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    public interface IViewerRepository
    {
        /// <summary>
        /// AddViewer takes a viewer object and inserts into the database
        /// </summary>
        /// <param name="viewer"></param>
        /// <returns></returns>
        Task<Viewer> AddViewer(Viewer viewer);
    }
}
