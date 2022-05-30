// <copyright file="TutorialGroupConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// <see cref="TutorialGroupEntity"/> configuration.
    /// </summary>
    internal sealed class TutorialGroupConfiguration : IEntityTypeConfiguration<TutorialGroupEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<TutorialGroupEntity> builder)
        {
            // Properties
            builder.HasKey(t => t.Id);
            builder.Property(t => t.DisplayName)
                .IsRequired()
                .HasMaxLength(100); // Define constant.

            builder.Property(t => t.ShortCode)
                .IsRequired()
                .HasMaxLength(24); // Define constant.

            // Add indices for course & displayname/shortcode
            // for uniqueness checks on insert / update.
            builder.HasIndex(t => new { t.CourseId, t.DisplayName })
                .IsUnique();

            builder.HasIndex(t => new { t.CourseId, t.ShortCode })
                .IsUnique();

            // Many-Many Indirect Relationship.
            builder.HasMany(t => t.Members)
                .WithOne(tgm => tgm.TutorialGroup)
                .HasForeignKey(tgm => tgm.TutorialGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
