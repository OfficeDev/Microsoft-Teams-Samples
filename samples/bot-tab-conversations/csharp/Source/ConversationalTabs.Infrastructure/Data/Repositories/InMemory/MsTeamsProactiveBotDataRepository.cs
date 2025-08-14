// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Infrastructure.Data.Repositories.InMemory;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Exceptions;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.IRepositories;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public class MsTeamsBotDataRepository : IRepository<MsTeamsBotData>
{
    private readonly IDictionary<string, MsTeamsBotData> _proactiveBotDataDictionary;

    public MsTeamsBotDataRepository()
    {
        _proactiveBotDataDictionary = new ConcurrentDictionary<string, MsTeamsBotData>();
    }

    public Task<MsTeamsBotData> CreateOrUpdateObject(MsTeamsBotData input)
    {
        if (_proactiveBotDataDictionary.ContainsKey(input.Id))
        {
            _proactiveBotDataDictionary[input.Id] = input;
        }
        else
        {
            _proactiveBotDataDictionary.Add(input.Id, input);
        }

        return Task.FromResult(input);
    }

    public Task<bool> DeleteObject(MsTeamsBotData item) => throw new NotImplementedException();

    public Task<MsTeamsBotData> GetObject(string id)
    {
        if (_proactiveBotDataDictionary.TryGetValue(id, out MsTeamsBotData channel))
        {
            return Task.FromResult(channel);
        }

        throw new ApiException(HttpStatusCode.BadRequest, ErrorCode.ChannelActivityNotFound, $"Requested channel activity with id {id} was not found.");
    }

    public Task<ICollection<MsTeamsBotData>> GetAllObjects() => throw new NotImplementedException();
}
