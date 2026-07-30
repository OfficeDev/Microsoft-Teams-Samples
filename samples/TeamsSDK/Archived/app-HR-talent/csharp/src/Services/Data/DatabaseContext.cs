using System;
using Microsoft.EntityFrameworkCore;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Recruiter> Recruiters { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<SubscribeEvent> SubscribeEvents { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<ConversationData> ConversationData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseInMemoryDatabase(typeof(DatabaseContext).FullName ?? Guid.NewGuid().ToString());
        }
    }
}
