// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure
{
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entitites;

    public class MeetingSigningDbContext : DbContext
    {
        public MeetingSigningDbContext(DbContextOptions<MeetingSigningDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<DocumentEntity> Documents { get; set; }

        public DbSet<SignatureEntity> Signatures { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<ViewerEntity> Viewers { get; set; }
    }
}
