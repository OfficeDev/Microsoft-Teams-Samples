// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.IRepositories;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public class MsTeamsBotDataService : IRepositoryObjectService<MsTeamsBotData, MsTeamsBotData>
{
    private readonly IRepository<MsTeamsBotData> _msTeamsBotDataRepository;
    private readonly ILogger<MsTeamsBotData> _logger;

    public MsTeamsBotDataService(IRepository<MsTeamsBotData> msTeamsBotDataRepository, ILogger<MsTeamsBotData> logger)
    {
        _msTeamsBotDataRepository = msTeamsBotDataRepository;
        _logger = logger;
    }

    public Task<MsTeamsBotData> Create(MsTeamsBotData input)
    {
        return _msTeamsBotDataRepository.CreateOrUpdateObject(input);
    }

    public Task<MsTeamsBotData> GetSingle(string id) => throw new NotImplementedException();
    public Task<ICollection<MsTeamsBotData>> GetAll() => throw new NotImplementedException();
}
