using Newtonsoft.Json;
using System;

namespace Raven.Migrations
{
    public class MigrationInfo
    {
        public string Id { get; set; }
        public long Version { get; set; }
        [JsonIgnore]
        public IMigration Migration { get; set; }

        public static MigrationInfo CreateNew(long version, Type migrationType)
        {
            return new MigrationInfo
            {
                Id = "migrationinfos/" + version,
                Version = version,
                Migration = (IMigration)Activator.CreateInstance(migrationType)
            };
        }
    }
}
