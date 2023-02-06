namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// Course entity configuration.
    /// </summary>
    internal sealed class CourseConfiguration : IEntityTypeConfiguration<CourseEntity>
    {
        private const int MaxTeamNameLength = 255;

        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<CourseEntity> builder)
        {
            // Properties
            builder.HasKey(c => c.Id);
            builder.Property(c => c.TeamAadObjectId).IsRequired();
            builder.HasIndex(c => c.TeamAadObjectId).IsUnique();
            builder.Property(c => c.TeamId).IsRequired();
            builder.HasIndex(c => c.TeamId).IsUnique();
            builder.Property(c => c.Name).IsRequired().HasMaxLength(MaxTeamNameLength);

            // 1 : Many Relationships
            builder
                .HasMany(c => c.Channels)
                .WithOne()
                .HasForeignKey(channel => channel.CourseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(c => c.TutorialGroups)
                .WithOne()
                .HasForeignKey(t => t.CourseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Many : Many Indirect Relationship.
            builder
                .HasMany(c => c.Members)
                .WithOne(m => m.Course)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
