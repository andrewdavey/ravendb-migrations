using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Raven.Client;
using Raven.Client.Document;
using Xunit;

namespace Raven.Migrations
{
    public class MigratorTests : IDisposable
    {
        public MigratorTests()
        {
            store = new DocumentStore();
            store.RunInMemory = true;
            store.Initialize();

            migrator = new Migrator();
        }

        public void Dispose()
        {
            store.Dispose();
        }

        readonly DocumentStore store;
        readonly Migrator migrator;

        [Fact]
        public void Migrate_UpToMax()
        {
            migrator.Migrate(store, new FakeAssembly(typeof(Migration1)));

            Assert.NotNull(store.DatabaseCommands.GetIndex("BooksByTitle"));
        }

        [Fact]
        public void Migrate_UpTo1()
        {
            migrator.Migrate(store, new FakeAssembly(typeof(Migration1)), 1);

            Assert.NotNull(store.DatabaseCommands.GetIndex("BooksByTitle"));
        }

        [Fact]
        public void Migrate_UpTo2()
        {
            migrator.Migrate(store, new FakeAssembly(typeof(Migration1), typeof(Migration2)), 2);

            Assert.NotNull(store.DatabaseCommands.GetIndex("BooksByTitle"));
            Assert.NotNull(store.DatabaseCommands.GetIndex("BooksByAuthor"));
        }

        [Fact]
        public void Migrate_DownTo0()
        {
            migrator.Migrate(store, new FakeAssembly(typeof(Migration1)));
            WaitForIndexing();

            migrator.Migrate(store, new FakeAssembly(typeof(Migration1)), 0);

            Assert.Null(store.DatabaseCommands.GetIndex("BooksByTitle"));
        }

        [Fact]
        public void Migrate_WithMigrationThatThrows_RollsbackTransaction()
        {
            try
            {
                migrator.Migrate(store, new FakeAssembly(typeof(Migration1), typeof(MigrationThatThrows)));
            }
            catch (Exception)
            {
            }
            WaitForIndexing();

            Assert.Null(store.DatabaseCommands.GetIndex("BooksByTitle"));
        }

        [Fact]
        public void Migrate_From1ToMigrationThatThrows_RollsbackTo1()
        {
            migrator.Migrate(store, new FakeAssembly(typeof(Migration1)));
            WaitForIndexing();

            try
            {
                migrator.Migrate(store, new FakeAssembly(typeof(MigrationThatThrows)));
            }
            catch
            {
            }
            WaitForIndexing();

            Assert.NotNull(store.DatabaseCommands.GetIndex("BooksByTitle"));
            Assert.NotNull(store.DatabaseCommands.Get("migrationinfos/1"));
        }

        [Fact]
        public void Migrate_NoMigrationsDefined_Throws()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                migrator.Migrate(store, new FakeAssembly(), 1);
            });
        }

        [Fact]
        public void Migrate_ToMaxNoMigrationsDefined_DoesNothing()
        {
            migrator.Migrate(store, new FakeAssembly());
        }

        [Fact]
        public void Migrate_ToVersionThatHasNoMigration_Throws()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                migrator.Migrate(store, new FakeAssembly(typeof(Migration1)), 2);
            });
        }

        [Fact]
        public void Migrate_DownToVersionThatHasNoMigration_Throws()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                migrator.Migrate(store, new FakeAssembly(typeof(Migration2)), 1);
            });
        }

        void WaitForIndexing()
        {
            Assert.Equal(0, store.DocumentDatabase.Statistics.ApproximateTaskCount);
            while (store.DocumentDatabase.Statistics.StaleIndexes.Length > 0)
            {
                Debug.WriteLine("waiting for indexes...");
                Thread.Sleep(100);
            }
        }

    }

    class FakeAssembly : Assembly
    {
        public FakeAssembly(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; set; }

        public override Type[] GetExportedTypes()
        {
            return Types;
        }

        public override string FullName
        {
            get
            {
                return "FakeAssembly";
            }
        }
    }


    class Book
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }

    [Migration(1)]
    public class Migration1 : IMigration
    {
        public void Up(IDocumentSession session)
        {
            session.Advanced.DatabaseCommands.PutIndex<Book, Book>("BooksByTitle", new Client.Indexes.IndexDefinition<Book, Book>
            {
                Map = books => from b in books
                               select new { b.Title }
            });
        }

        public void Down(IDocumentSession session)
        {
            session.Advanced.DatabaseCommands.DeleteIndex("BooksByTitle");
        }
    }

    [Migration(2)]
    public class Migration2 : IMigration
    {
        public void Up(IDocumentSession session)
        {
            session.Advanced.DatabaseCommands.PutIndex<Book, Book>("BooksByAuthor", new Client.Indexes.IndexDefinition<Book, Book>
            {
                Map = books => from b in books
                               select new { b.Author }
            });
        }

        public void Down(IDocumentSession session)
        {
            session.Advanced.DatabaseCommands.DeleteIndex("BooksByAuthor");
        }
    }

    [Migration(3)]
    public class MigrationThatThrows : IMigration
    {
        public void Up(IDocumentSession session)
        {
            throw new Exception();
        }

        public void Down(IDocumentSession session)
        {
        }
    }

}
