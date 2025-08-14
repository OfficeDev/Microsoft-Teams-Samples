namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// <see cref="CourseMemberEntity"/> configuration.
    ///
    /// <see cref="MemberRepository"/> manages course members.
    /// </summary>
    internal sealed class CourseMemberConfiguration : IEntityTypeConfiguration<CourseMemberEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<CourseMemberEntity> builder)
        {
            // Properties
            builder.HasKey(m => new { m.CourseId, m.UserId });
            builder.Property(m => m.MemberRole).IsRequired();

            // Many-Many Indirect Relationship.
            builder.HasMany(cm => cm.TutorialGroupsMembership)
                .WithOne()
                .HasForeignKey(t => new { t.CourseId, t.UserId })
                .OnDelete(DeleteBehavior.NoAction); // Note: make sure to manually delete this when member is removed.
        }
    }
}
