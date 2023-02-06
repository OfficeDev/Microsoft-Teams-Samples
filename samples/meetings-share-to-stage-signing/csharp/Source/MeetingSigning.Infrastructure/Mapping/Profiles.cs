// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Mapping
{
    using AutoMapper;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entitites;

    public class Profiles : Profile
    {
        public Profiles()
        {
            this.CreateMap<User, UserEntity>();
            this.CreateMap<UserEntity, User>()
                .ForSourceMember(u => u.DocumentViewers, e => e.DoNotValidate());

            this.CreateMap<SignatureEntity, Signature>()
                .ForSourceMember(s => s.Signer, opt => opt.DoNotValidate());
            this.CreateMap<Signature, SignatureEntity>()
                .ForSourceMember(s => s.Signer, opt => opt.DoNotValidate());

            this.CreateMap<Document, DocumentEntity>()
                .ForSourceMember(e=> e.Signatures, opt=>opt.DoNotValidate());

            this.CreateMap<Viewer, ViewerEntity>();
            this.CreateMap<ViewerEntity, Viewer>();
        }
    }
}
