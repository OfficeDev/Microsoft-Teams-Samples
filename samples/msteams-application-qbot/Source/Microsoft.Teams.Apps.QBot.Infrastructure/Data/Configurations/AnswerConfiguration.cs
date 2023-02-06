namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// <see cref="AnswerEntity"/> configuration.
    /// </summary>
    internal sealed class AnswerConfiguration : IEntityTypeConfiguration<AnswerEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<AnswerEntity> builder)
        {
            // Properties
            builder.HasKey(a => a.Id);

            builder.Property(a => a.CourseId)
                .IsRequired();
            builder.Property(a => a.ChannelId)
                .IsRequired();
            builder.Property(a => a.MessageId)
                .IsRequired();
            builder.Property(a => a.AuthorId)
                .IsRequired();
            builder.Property(a => a.AcceptedById)
                .IsRequired();
        }
    }
}
