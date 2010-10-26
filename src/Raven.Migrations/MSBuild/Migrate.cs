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
            ToVersion = -1;
        }

        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public ITaskItem[] Migrations { get; set; }
        
        public long ToVersion { get; set; }

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
                    migrator.Migrate(store, assembly, ToVersion);
                }
            }

            if (ToVersion > 0)
            {
                Log.LogMessage("Migrated to version " + ToVersion + ".");
            }
            else
            {
                Log.LogMessage("Migrated to maximum version.");
            }
            return true;
        }
    }
}
