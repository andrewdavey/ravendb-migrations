using Newtonsoft.Json;

namespace Raven.Migrations
{
    public class MigrationInfo
    {
        public string Id { get; set; }
        public long Version { get; set; }
        [JsonIgnore]
        public IMigration Migration { get; set; }
    }
}
