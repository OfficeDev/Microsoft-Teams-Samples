namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data
{
    using System.Linq;
    using AutoMapper;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// AutoMapper Profile to map Entitites to Domain Objects and vice-versa.
    /// </summary>
    public class Profiles : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Profiles"/> class.
        /// </summary>
        public Profiles()
        {
            // Course <-> CourseEntity
            this.CreateMap<CourseEntity, Course>()
                .ForMember(d => d.HasTutorialGroups, e => e.MapFrom(s => s.TutorialGroups.Any()))
                .ForSourceMember(s => s.Members, e => e.DoNotValidate())
                .ForSourceMember(s => s.TutorialGroups, e => e.DoNotValidate());

            this.CreateMap<Course, CourseEntity>();

            // Channel <-> Channel Entity
            this.CreateMap<ChannelEntity, Channel>();
            this.CreateMap<Channel, ChannelEntity>();

            // User <-> UserEntity
            this.CreateMap<User, UserEntity>()
                .ForSourceMember(s => s.ProfilePicUrl, e => e.DoNotValidate());

            this.CreateMap<UserEntity, User>()
                .ForSourceMember(s => s.CourseMembership, e => e.DoNotValidate())
                .ForSourceMember(s => s.TutorialGroupMembership, e => e.DoNotValidate())
                .ForSourceMember(s => s.KnowledgeBases, e => e.DoNotValidate())
                .ForMember(d => d.ProfilePicUrl, opt => opt.Ignore());

            // Member <-> UserEntity.
            this.CreateMap<UserEntity, Member>()
                .ForSourceMember(s => s.CourseMembership, e => e.DoNotValidate())
                .ForSourceMember(s => s.TutorialGroupMembership, e => e.DoNotValidate())
                .ForSourceMember(s => s.KnowledgeBases, e => e.DoNotValidate())
                .ForMember(d => d.Role, opt => opt.Ignore());

            this.CreateMap<Member, UserEntity>()
                .ForSourceMember(s => s.Role, e => e.DoNotValidate())
                .ForSourceMember(s => s.TutorialGroupMembership, e => e.DoNotValidate());

            // Tutorial Group <-> Tutorial Group Entity.
            this.CreateMap<TutorialGroupEntity, TutorialGroup>()
                .ForSourceMember(s => s.Members, e => e.DoNotValidate());
            this.CreateMap<TutorialGroup, TutorialGroupEntity>();

            // Question <-> Question Entity.
            this.CreateMap<Question, QuestionEntity>();
            this.CreateMap<QuestionEntity, Question>()
                .ForSourceMember(s => s.Answer, e => e.DoNotValidate());

            // Answer <-> Answer Entity.
            this.CreateMap<Answer, AnswerEntity>()
                .ForSourceMember(s => s.Message, e => e.DoNotValidate());
            this.CreateMap<AnswerEntity, Answer>()
                .ForSourceMember(s => s.Question, e => e.DoNotValidate());

            // KB <-> KB Entity.
            this.CreateMap<KnowledgeBase, KnowledgeBaseEntity>();
            this.CreateMap<KnowledgeBaseEntity, KnowledgeBase>();
        }
    }
}
