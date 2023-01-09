// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Data.Repositories
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;
    using Microsoft.Teams.Samples.MeetingSigning.Infrastructure.Entities;

    public class UserRepository : IUserRepository
    {
        private readonly MeetingSigningDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserRepository(MeetingSigningDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task AddUser(User user)
        {
            try
            {
                var userEntity = _mapper.Map<UserEntity>(user);
                var exists = await UserExists(userEntity.Id);
                if (!exists)
                {
                    _dbContext.Users.Add(userEntity);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                //ToDo log exception
                throw;
            }
        }

        public async Task<User> GetUser(string id)
        {
            try
            {
                var userEntity = await _dbContext.Users
                    .Where(u => u.UserId.Equals(id, StringComparison.Ordinal) || u.Email.Equals(id, StringComparison.Ordinal))
                    .FirstOrDefaultAsync();
                return this._mapper.Map<User>(userEntity);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UserExists(string id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            //returns a null if a match isn't found
            return user != null;
        }
    }
}
