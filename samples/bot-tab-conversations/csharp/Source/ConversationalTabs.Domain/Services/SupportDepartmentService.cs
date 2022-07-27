// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.IRepositories;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public class SupportDepartmentService : IRepositoryObjectService<SupportDepartment, SupportDepartmentInput>
{
    private readonly IRepository<MsTeamsBotData> _proactiveBotDataRepository;
    private readonly IRepository<SupportDepartment> _supportDepartmentRepository;
    private readonly ILogger<SupportDepartmentService> _logger;

    public SupportDepartmentService(IRepository<SupportDepartment> supportDepartmentRepository, IRepository<MsTeamsBotData> proactiveBotDataRepository, ILogger<SupportDepartmentService> logger)
    {
        _proactiveBotDataRepository = proactiveBotDataRepository;
        _supportDepartmentRepository = supportDepartmentRepository;
        _logger = logger;
    }

    public async Task<SupportDepartment> Create(SupportDepartmentInput input)
    {
        var proactiveBotData = await _proactiveBotDataRepository.GetObject(input.TeamId);

        var entityId = Guid.NewGuid().ToString();
        SupportDepartment category = new(entityId, input.Title, input.Description, input.GroupId, input.TeamChannelId, input.TenantId, proactiveBotData, new List<CustomerInquiry>());

        return await _supportDepartmentRepository.CreateOrUpdateObject(category);
    }

    public Task<ICollection<SupportDepartment>> GetAll()
    {
        return _supportDepartmentRepository.GetAllObjects();
    }

    public Task<SupportDepartment> GetSingle(string id)
    {
        return _supportDepartmentRepository.GetObject(id);
    }
}
