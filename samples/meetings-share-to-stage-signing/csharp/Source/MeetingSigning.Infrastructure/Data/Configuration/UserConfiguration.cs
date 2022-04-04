// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities;

    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            // Properties
            builder.HasKey( u => u.UserId);

            // One to Many relationship
            builder.HasMany(u => u.DocumentViewers)
                .WithOne(c => c.Viewer)
                .HasForeignKey( u => u.UserId );
        }
    }
}
