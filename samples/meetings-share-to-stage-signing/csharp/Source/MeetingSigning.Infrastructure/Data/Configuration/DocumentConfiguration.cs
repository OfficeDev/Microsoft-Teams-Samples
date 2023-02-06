// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities;

    public class DocumentConfiguration : IEntityTypeConfiguration<DocumentEntity>
    {
        public void Configure(EntityTypeBuilder<DocumentEntity> builder)
        {
            // Properties
            builder.HasKey(c => c.Id);

            // One to Many Relationship
            builder
                .HasMany(c => c.DocumentViewers)
                .WithOne()
                .HasForeignKey(document => document.DocumentId)
                ;

            builder
                .HasMany(c => c.SignatureEntities);
        }
    }
}
