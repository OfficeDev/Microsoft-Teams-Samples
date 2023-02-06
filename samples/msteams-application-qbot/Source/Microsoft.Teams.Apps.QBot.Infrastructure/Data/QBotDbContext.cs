namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// QBot DBContext instance with the database.
    /// </summary>
    internal class QBotDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QBotDbContext"/> class.
        /// </summary>
        /// <param name="options">Db Context options.</param>
        public QBotDbContext(DbContextOptions<QBotDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets <see cref="CourseEntity"/> DbSet.
        /// </summary>
        public DbSet<CourseEntity> Courses { get; set; }

        /// <summary>
        /// Gets or sets <see cref="ChannelEntity"/> DbSet.
        /// </summary>
        public DbSet<ChannelEntity> Channels { get; set; }

        /// <summary>
        /// Gets or sets <see cref="UserEntity"/> DbSet.
        /// </summary>
        public DbSet<UserEntity> Users { get; set; }

        /// <summary>
        /// Gets or sets <see cref="TutorialGroupEntity"/> DbSet.
        /// </summary>
        public DbSet<TutorialGroupEntity> TutorialGroups { get; set; }

        /// <summary>
        /// Gets or sets <see cref="QuestionEntity"/> DbSet.
        /// </summary>
        public DbSet<QuestionEntity> Questions { get; set; }

        /// <summary>
        /// Gets or sets <see cref="AppSettingEntity"/> DbSet.
        /// </summary>
        public DbSet<AppSettingEntity> AppSettings { get; set; }

        /// <summary>
        /// Gets or sets <see cref="KnowledgeBaseEntity"/> DbSet.
        /// </summary>
        public DbSet<KnowledgeBaseEntity> KnowledgeBases { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
