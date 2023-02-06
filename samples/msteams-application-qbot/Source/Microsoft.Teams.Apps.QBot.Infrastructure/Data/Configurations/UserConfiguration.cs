namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// <see cref="UserEntity"/> configuration.
    /// </summary>
    internal sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            // Properties
            builder.HasKey(u => u.AadId);
            builder.HasIndex(u => u.TeamId).IsUnique();
            builder.Property(u => u.Name).IsRequired();
            builder.Property(u => u.Upn).IsRequired();

            // Many-Many Indirect relationships.
            builder
                .HasMany(u => u.CourseMembership)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(u => u.TutorialGroupMembership)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // 1-Many Relationship.
            builder
                .HasMany(u => u.KnowledgeBases)
                .WithOne()
                .HasForeignKey(k => k.OwnerUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
