namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// <see cref="TutorialGroupMemberEntity"/> configuration.
    /// </summary>
    internal sealed class TutorialGroupMemberConfiguration : IEntityTypeConfiguration<TutorialGroupMemberEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<TutorialGroupMemberEntity> builder)
        {
            // Properties
            builder.HasKey(m => new { m.CourseId, m.TutorialGroupId, m.UserId });
            builder.Property(m => m.CourseId)
                .IsRequired();
        }
    }
}
