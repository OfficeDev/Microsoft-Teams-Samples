// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities;

    public class DocumentViewerConfiguration : IEntityTypeConfiguration<DocumentViewerEntity>
    {
        public void Configure(EntityTypeBuilder<DocumentViewerEntity> builder)
        {
            // Properties
            builder.HasKey(m => new { m.DocumentId, m.UserId });

            // One to Many Relationships
            builder.HasOne<DocumentEntity>(d => d.Document).
                WithMany(r => r.DocumentViewers).HasForeignKey(d => d.DocumentId);
            builder.HasOne<UserEntity>(u => u.Viewer).
                WithMany(r => r.DocumentViewers).HasForeignKey(u => u.UserId);
        }
    }
}
