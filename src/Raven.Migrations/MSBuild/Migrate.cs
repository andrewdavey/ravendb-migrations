using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Raven.Client.Document;

namespace Raven.Migrations.MSBuild
{
    public class Migrate : Task
    {
        public Migrate()
        {
            SchemaVersion = -1;
        }

        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public ITaskItem[] Migrations { get; set; }
        
        public long SchemaVersion { get; set; }

        public override bool Execute()
        {
            using (var store = new DocumentStore())
            {
                store.ConfigureUsingConnectionString(ConnectionString);
                store.Initialize();
                
                var migrator = new Migrator();
                foreach (var item in Migrations)
                {
                    var assembly = Assembly.LoadFrom(item.GetMetadata("FullPath"));
                    migrator.Migrate(store, assembly, SchemaVersion);
                }
            }
            Log.LogMessage("Migrated to version " + SchemaVersion);
            return true;
        }
    }
}
