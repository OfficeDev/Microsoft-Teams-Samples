// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entitites;

    public class ViewerRepository : IViewerRepository
    {
        private readonly MeetingSigningDbContext dbContext;
        private readonly IMapper mapper;

        public ViewerRepository(MeetingSigningDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Viewer> AddViewer(Viewer viewer)
        {
            var existingViewer = await this.dbContext.Users.FirstOrDefaultAsync(s => s.UserId.Equals(viewer.Observer.UserId));
            if (existingViewer != null)
            {
                var viewerEntity = this.mapper.Map<ViewerEntity>(viewer);
                viewerEntity.Observer = existingViewer;
                dbContext.Viewers.Add(viewerEntity);
                await dbContext.SaveChangesAsync();
                return this.mapper.Map<Viewer>(viewerEntity);
            }
            else
            {
                throw new ApiException(HttpStatusCode.InternalServerError, ErrorCode.Unknown);
            }
        }
    }
}
