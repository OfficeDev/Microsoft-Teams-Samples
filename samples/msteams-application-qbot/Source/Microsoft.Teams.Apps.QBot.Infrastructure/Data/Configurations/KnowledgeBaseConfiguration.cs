namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// <see cref="KnowledgeBaseEntity"/> configuration.
    /// </summary>
    internal sealed class KnowledgeBaseConfiguration : IEntityTypeConfiguration<KnowledgeBaseEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<KnowledgeBaseEntity> builder)
        {
            // Properties
            builder.HasKey(k => k.Id);

            builder.Property(k => k.Name)
                .IsRequired();
            builder.Property(k => k.OwnerUserId)
                .IsRequired();

            // 1 : Many relationship.
            builder
                .HasMany(k => k.Courses)
                .WithOne()
                .HasForeignKey(c => c.KnowledgeBaseId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
