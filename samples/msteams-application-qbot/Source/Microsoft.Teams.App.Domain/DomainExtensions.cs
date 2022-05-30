namespace Microsoft.Teams.Apps.QBot.Domain
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.QBot.Domain.Courses;
    using Microsoft.Teams.Apps.QBot.Domain.KnowledgeBases;
    using Microsoft.Teams.Apps.QBot.Domain.Questions;
    using Microsoft.Teams.Apps.QBot.Domain.Users;

    /// <summary>
    /// Domain Extension class.
    /// </summary>
    public static class DomainExtensions
    {
        /// <summary>
        /// Service Collection extension.
        /// </summary>
        /// <param name="services">Servie collection.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            // Validators.
            services.AddTransient<ICourseValidator, CourseValidator>();
            services.AddTransient<ITutorialGroupValidator, TutorialGroupValidator>();
            services.AddTransient<IQuestionValidator, QuestionValidator>();
            services.AddTransient<IAnswerValidator, AnswerValidator>();
            services.AddTransient<IKnowledgeBaseValidator, KnowledgeBaseValidator>();

            // Services.
            services.AddTransient<ICourseReader, CourseReader>();
            services.AddTransient<ICourseSetup, CourseSetup>();
            services.AddTransient<ITutorialGroupSetup, TutorialGroupSetup>();
            services.AddTransient<QBotService>();
            services.AddTransient<IQuestionReader>(sp => sp.GetRequiredService<QBotService>());
            services.AddTransient<IQBotService>(sp => sp.GetRequiredService<QBotService>());
            services.AddTransient<IKnowledgeBaseWriter, KnowledgeBaseWriter>();
            services.AddTransient<IKnowledgeBaseReader, KnowledgeBaseReader>();
            services.AddTransient<IUserReaderService, UserReaderService>();
            return services;
        }
    }
}
