using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Transactions;
using Raven.Client.Document;
using Raven.Client;

namespace Raven.Migrations
{
    public class Migrator
    {
        public void Migrate(IDocumentStore store, Assembly assemblyContainingMigrations, int toVersion = -1)
        {
            store.Initialize();

            var appliedMigrations = GetAppliedMigrations(store);
            var appliedVersions = new HashSet<long>(appliedMigrations.Select(m => m.Version));
            var currentMaxVersion = appliedVersions.Count == 0 ? 0 : appliedVersions.Max();
            var migrationTypes = GetMigrationTypes(assemblyContainingMigrations);

            IEnumerable<MigrationInfo> migrationsToRun;
            bool up;
            if (toVersion < 0)
            {
                up = true;
                var filter = UpToMaxVersionFilter(appliedVersions);
                migrationsToRun =
                    from t in migrationTypes
                    where filter(t.Key)
                    select new MigrationInfo
                    {
                        Version = t.Key,
                        Migration = (IMigration)Activator.CreateInstance(t.Value)
                    };

            }
            else if (toVersion > currentMaxVersion)
            {
                up = true;
                var filter = UpToVersionFilter(toVersion, appliedVersions);
                migrationsToRun =
                    from t in migrationTypes
                    where filter(t.Key)
                    select new MigrationInfo
                    {
                        Version = t.Key,
                        Migration = (IMigration)Activator.CreateInstance(t.Value)
                    };
            }
            else
            {
                up = false;
                var filter = DownToVersionFilter(toVersion, appliedVersions);
                migrationsToRun = appliedMigrations.Where(m => filter(m.Version)).ToArray();
                foreach (var m in migrationsToRun)
                {
                    m.Migration = (IMigration)Activator.CreateInstance(migrationTypes[m.Version]);
                }
            }

            foreach (var item in migrationsToRun)
            {
                if (up)
                {
                    item.Migration.Up(store);
                    using (var session = store.OpenSession())
                    {
                        session.Store(item);
                        session.SaveChanges();
                    }
                }
                else
                {
                    item.Migration.Down(store);
                    store.DatabaseCommands.Delete(item.Id, null);
                }
            }
        }

        Func<long, bool> UpToVersionFilter(int toVersion, ISet<long> appliedVersions)
        {
            return v => !appliedVersions.Contains(v) && v <= toVersion;
        }

        Func<long, bool> DownToVersionFilter(int toVersion, ISet<long> appliedVersions)
        {
            return v => appliedVersions.Contains(v) && v > toVersion;
        }

        Func<long, bool> UpToMaxVersionFilter(ISet<long> appliedVersions)
        {
            return v => !appliedVersions.Contains(v);
        }

        IEnumerable<MigrationInfo> GetAppliedMigrations(IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                return session.Advanced.LuceneQuery<MigrationInfo>().WaitForNonStaleResults().ToArray();
            }
        }

        Dictionary<long, Type> GetMigrationTypes(Assembly assemblyContainingMigrations)
        {
            var migrationMetadata =
                from type in assemblyContainingMigrations.GetExportedTypes()
                where typeof(IMigration).IsAssignableFrom(type)
                let attr = (MigrationAttribute)type.GetCustomAttributes(typeof(MigrationAttribute), false).FirstOrDefault()
                select new { type, attr };

            foreach (var metadatum in migrationMetadata)
            {
                if (metadatum.attr == null) 
                    throw new InvalidOperationException("The Migration attribute is missing from " + metadatum.type.FullName);
            }

            var migrationTypes = migrationMetadata.ToDictionary(
                m => m.attr.Version, // key
                m => m.type // value
            );
            return migrationTypes;
        }

        // Copied (and tweaked a bit) from RavenDB source: DocumentStore.cs
        static readonly Regex connectionStringRegex = new Regex(@"(\w+) \s* = \s* (.*)",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        static readonly Regex connectionStringArgumentsSplitterRegex = new Regex(@"; (?=\s* \w+ \s* =)",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        void Configure(DocumentStore store, string connectionString)
        {
            // Copied (and tweaked a bit) from RavenDB source: DocumentStore.cs
            string user = null;
            string pass = null;
            var strings = connectionStringArgumentsSplitterRegex.Split(connectionString);
            foreach (var arg in strings)
            {
                var match = connectionStringRegex.Match(arg);
                if (match.Success == false)
                    throw new ArgumentException("Connection string could not be parsed");
                switch (match.Groups[1].Value.ToLower())
                {
                    case "memory":
                        bool result;
                        if (bool.TryParse(match.Groups[2].Value, out result) == false)
                            throw new ArgumentException("Could not understand memory setting: " +
                                match.Groups[2].Value);
                        store.RunInMemory = result;
                        break;
                    case "datadir":
                        store.DataDirectory = match.Groups[2].Value.Trim();
                        break;
                    case "resourcemanagerid":
                        store.ResourceManagerId = new Guid(match.Groups[2].Value.Trim());
                        break;
                    case "url":
                        store.Url = match.Groups[2].Value.Trim();
                        break;

                    case "user":
                        user = match.Groups[2].Value.Trim();
                        break;
                    case "password":
                        pass = match.Groups[2].Value.Trim();
                        break;

                    default:
                        throw new ArgumentException("Connection string could not be parsed, unknown option: " + match.Groups[1].Value);
                }
            }

            if (user == null && pass == null)
                return;

            if (user == null || pass == null)
                throw new ArgumentException("User and Password must both be specified in the connection string");
            store.Credentials = new NetworkCredential(user, pass);
        }
    }
}
