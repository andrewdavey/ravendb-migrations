using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Raven.Client;

namespace Raven.Migrations
{
    public class Migrator
    {
        public void Migrate(IDocumentStore store, Assembly assemblyContainingMigrations, long toVersion = -1)
        {
            if (toVersion < 0) toVersion = long.MaxValue;
            using (var session = new IndexSavingDocumentSession(store.OpenSession()))
            {
                var txId = Guid.NewGuid();
                session.Advanced.DatabaseCommands.PromoteTransaction(txId);
                try
                {
                    var appliedMigrations = GetAppliedMigrations(store);
                    var appliedVersions = new HashSet<long>(appliedMigrations.Select(m => m.Version));
                    var currentMaxVersion = appliedVersions.Count == 0 ? 0 : appliedVersions.Max();
                    var migrationTypes = GetMigrationTypes(assemblyContainingMigrations);

                    if (toVersion > currentMaxVersion)
                    {
                        MigrateUpTo(toVersion, appliedVersions, migrationTypes, session);
                    }
                    else
                    {
                        MigrateDownTo(toVersion, appliedMigrations, appliedVersions, migrationTypes, session);
                    }

                    session.SaveChanges();
                    session.Advanced.DatabaseCommands.Commit(txId);
                }
                catch
                {
                    session.RestoreIndexes();
                    session.Advanced.DatabaseCommands.Rollback(txId);
                    throw;
                }
            }
        }

        void MigrateUpTo(long version, ISet<long> appliedVersions, IDictionary<long, Type> migrationTypes, IDocumentSession session)
        {
            var filter = UpToVersionFilter(version, appliedVersions);
            var migrationsToRun =
                from t in migrationTypes
                where filter(t.Key)
                orderby t.Key
                select new MigrationInfo
                {
                    Id = "migrationinfos/" + t.Key,
                    Version = t.Key,
                    Migration = (IMigration)Activator.CreateInstance(t.Value)
                };

            foreach (var item in migrationsToRun)
            {
                item.Migration.Up(session);
            }
            foreach (var item in migrationsToRun)
            {
                session.Store(item);
            }
        }

        void MigrateDownTo(long version, IEnumerable<MigrationInfo> appliedMigrations, ISet<long> appliedVersions, IDictionary<long, Type> migrationTypes, IDocumentSession session)
        {
            var filter = DownToVersionFilter(version, appliedVersions);
            var migrationsToRun = appliedMigrations.Where(m => filter(m.Version)).OrderByDescending(m => m.Version).ToArray();
            foreach (var m in migrationsToRun)
            {
                m.Migration = (IMigration)Activator.CreateInstance(migrationTypes[m.Version]);
            }
            foreach (var item in migrationsToRun)
            {
                item.Migration.Down(session);
            }
            foreach (var item in migrationsToRun)
            {
                session.Advanced.DatabaseCommands.Delete(item.Id, null);
            }
        }

        Func<long, bool> UpToVersionFilter(long toVersion, ISet<long> appliedVersions)
        {
            return v => !appliedVersions.Contains(v) && v <= toVersion;
        }

        Func<long, bool> DownToVersionFilter(long toVersion, ISet<long> appliedVersions)
        {
            return v => appliedVersions.Contains(v) && v > toVersion;
        }

        IEnumerable<MigrationInfo> GetAppliedMigrations(IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                return session.Advanced
                    .LuceneQuery<MigrationInfo>()
                    .WaitForNonStaleResults()
                    .ToArray();
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

       
    }
}
