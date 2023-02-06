namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// <see cref="QuestionEntity"/> configuration.
    /// </summary>
    internal sealed class QuestionConfiguration : IEntityTypeConfiguration<QuestionEntity>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<QuestionEntity> builder)
        {
            // Properties
            builder.HasKey(q => q.Id);
            builder.HasIndex(q => q.MessageId).IsUnique();
            builder.Property(q => q.MessageId).IsRequired();
            builder.Property(q => q.AuthorId).IsRequired();

            // 1:1 relationship.
            builder
                .HasOne(q => q.Answer)
                .WithOne(a => a.Question)
                .HasForeignKey<AnswerEntity>(a => a.QuestionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
