// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities;

    public class SignatureConfiguration : IEntityTypeConfiguration<SignatureEntity>
    {
        public void Configure(EntityTypeBuilder<SignatureEntity> builder)
        {
            // Properties
            builder.HasKey(s => s.Id);
        }
    }
}
