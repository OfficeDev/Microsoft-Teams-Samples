using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Data
{
    public static class SampleData
    {
        public static void InitializeDatabase(string dataSeedPath, DatabaseContext db)
        {
            var recruiters =
                JsonConvert.DeserializeObject<List<Recruiter>>(
                    File.ReadAllText(Path.Combine(dataSeedPath, "recruiters.json")));

            var candidates =
                JsonConvert.DeserializeObject<List<Candidate>>(
                    File.ReadAllText(Path.Combine(dataSeedPath, "candidates.json")));

            var positions =
                JsonConvert.DeserializeObject<List<Position>>(
                    File.ReadAllText(Path.Combine(dataSeedPath, "positions.json")));

            var locations =
                JsonConvert.DeserializeObject<List<Location>>(
                    File.ReadAllText(Path.Combine(dataSeedPath, "locations.json")));

            var interviews =
                JsonConvert.DeserializeObject<List<Interview>>(
                    File.ReadAllText(Path.Combine(dataSeedPath, "interviews.json")));

            db.Database.EnsureDeleted();

            db.Recruiters.AddRange(recruiters);
            db.Candidates.AddRange(candidates);
            db.Positions.AddRange(positions);
            db.Locations.AddRange(locations);
            db.Interviews.AddRange(interviews);

            db.SaveChanges();
        }
    }
}
