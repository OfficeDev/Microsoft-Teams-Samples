using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TeamsTalentMgmtApp.Models.DatabaseContext;
using TeamsTalentMgmtApp.Services.Interfaces;

namespace TeamsTalentMgmtApp.Services.Data
{
    public sealed class RecruiterService : IRecruiterService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IIncludableQueryable<Recruiter, ConversationData> _recruitersIncludableGetQuery;

        public RecruiterService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
            _recruitersIncludableGetQuery = _databaseContext.Recruiters
                .Include(x => x.Positions)
                .Include(x => x.ConversationData);
        }

        public async Task<Recruiter> GetById(int id, CancellationToken cancellationToken = default) =>
            await _recruitersIncludableGetQuery.FirstOrDefaultAsync(x => x.RecruiterId == id, cancellationToken);

        public async Task<ReadOnlyCollection<Recruiter>> GetAllHiringManagers(CancellationToken cancellationToken)
        {
            var result = await _recruitersIncludableGetQuery
                .Where(x => x.Role == RecruiterRole.HiringManager)
                .ToArrayAsync(cancellationToken);

            return Array.AsReadOnly(result);
        }

        public async Task<ReadOnlyCollection<Recruiter>> GetAllInterviewers(CancellationToken cancellationToken)
        {
            var result = await _recruitersIncludableGetQuery
                .Where(x => x.Role != RecruiterRole.HiringManager)
                .ToArrayAsync(cancellationToken);

            return Array.AsReadOnly(result);
        }

        public async Task SaveConversationData(string serviceUrl, string tenantId, Dictionary<string, string> channelAccountsEmails, CancellationToken cancellationToken)
        {
            var channelAccountsMap = new Dictionary<string, ConversationData>(channelAccountsEmails.Count);
            foreach (var channelAccountEmail in channelAccountsEmails)
            {
                var key = channelAccountEmail.Value
                    .Substring(0, channelAccountEmail.Value.IndexOf('@'))
                    .ToLowerInvariant();

                var channelData = new ConversationData
                {
                    AccountId = channelAccountEmail.Key,
                    ServiceUrl = serviceUrl,
                    TenantId = tenantId
                };

                channelAccountsMap.Add(key, channelData);
            }

            var aliases = channelAccountsMap.Keys.ToList();

            var recruiters = await _databaseContext.Recruiters
                .Where(x => aliases.Contains(x.Alias.ToLowerInvariant()))
                .ToArrayAsync(cancellationToken);

            foreach (var recruiter in recruiters)
            {
                recruiter.ConversationData = channelAccountsMap[recruiter.Alias.ToLowerInvariant()];
            }

            await _databaseContext.SaveChangesAsync(cancellationToken);
        }
    }
}
