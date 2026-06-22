using System.Net;
using AutoMapper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace TeamsTalentMgmtApp.Infrastructure
{
    public class TeamsTalentMgmtProfile : Profile
    {
        public TeamsTalentMgmtProfile()
        {
            CreateMap<MessagingExtensionAttachment[], InvokeResponse>()
                .ConstructUsing(src => new InvokeResponse
                {
                    Body = new MessagingExtensionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult
                        {
                            Type = "result",
                            Attachments = src,
                            AttachmentLayout = AttachmentLayoutTypes.List
                        }
                    },
                    Status = (int)HttpStatusCode.OK
                });

            CreateMap<MessagingExtensionAttachment[], MessagingExtensionResponse>()
                .ConstructUsing(src => new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "result",
                        Attachments = src,
                        AttachmentLayout = AttachmentLayoutTypes.List
                    }
                });

            CreateMap<MessagingExtensionResponse, MessagingExtensionActionResponse>()
                .ConstructUsing(src => new MessagingExtensionActionResponse
                {
                    ComposeExtension = src.ComposeExtension
                });

            CreateMap<MessagingExtensionActionResponse, InvokeResponse>()
                .ConstructUsing(src => new InvokeResponse { Body = src, Status = (int)HttpStatusCode.OK });

            CreateMap<MessagingExtensionResponse, InvokeResponse>()
                .ConstructUsing(src => new InvokeResponse { Body = src, Status = (int)HttpStatusCode.OK });

            CreateMap<MessagingExtensionResult, InvokeResponse>()
                .ConstructUsing((src, ctx) => ctx.Mapper.Map<MessagingExtensionResponse, InvokeResponse>(new MessagingExtensionResponse
                {
                    ComposeExtension = src
                }));
        }
    }
}
