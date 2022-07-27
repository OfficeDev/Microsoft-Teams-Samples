namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// <see cref="ChannelEntity"/> configuration.
    /// </summary>
    internal sealed class ChannelConfiguration : IEntityTypeConfiguration<ChannelEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<ChannelEntity> builder)
        {
            // Properties
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100); // Define constant.

            // One-Many Relationship.
            builder
                .HasMany(c => c.Questions)
                .WithOne()
                .HasForeignKey(q => q.ChannelId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
