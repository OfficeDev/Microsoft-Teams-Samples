// <copyright file="AppSettingConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// <see cref="AppSettingEntity"/> configuration.
    /// </summary>
    internal sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSettingEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<AppSettingEntity> builder)
        {
            builder.HasKey(s => s.Key);

            builder.Property(s => s.Value)
                .IsRequired();
        }
    }
}
