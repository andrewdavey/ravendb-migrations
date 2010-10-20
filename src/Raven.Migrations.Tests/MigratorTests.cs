using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Reflection;
using Moq;
using Raven.Client;
using Raven.Client.Document;

namespace Raven.Migrations
{
    public class MigratorTests
    {
        [Fact]
        public void Migrate_UpToMax()
        {
            using (var store = new DocumentStore())
            {
                store.RunInMemory = true;
                store.Initialize();

                var migrator = new Migrator();
                migrator.Migrate(store, new FakeAssembly
                {
                    Types = new[] {
                    typeof(Migration1)
                }
                });

                Assert.NotNull(store.DatabaseCommands.GetIndex("BooksByTitle"));
            }
        }

        [Fact]
        public void Migrate_UpTo1()
        {
            using (var store = new DocumentStore())
            {
                store.RunInMemory = true;
                store.Initialize();

                var migrator = new Migrator();
                migrator.Migrate(store, new FakeAssembly
                {
                    Types = new[] {
                    typeof(Migration1)
                }
                }, 1);

                Assert.NotNull(store.DatabaseCommands.GetIndex("BooksByTitle"));
            }
        }


        [Fact]
        public void Migrate_UpTo2()
        {
            using (var store = new DocumentStore())
            {
                store.RunInMemory = true;
                store.Initialize();

                var migrator = new Migrator();
                migrator.Migrate(store, new FakeAssembly
                {
                    Types = new[] { typeof(Migration1), typeof(Migration2) }
                }, 2);

                Assert.NotNull(store.DatabaseCommands.GetIndex("BooksByTitle"));
                Assert.NotNull(store.DatabaseCommands.GetIndex("BooksByAuthor"));
            }
        }

        [Fact]
        public void Migrate_DownTo0()
        {
            using (var store = new DocumentStore())
            {
                store.RunInMemory = true;
                store.Initialize();

                var migrator = new Migrator();
                migrator.Migrate(store, new FakeAssembly
                {
                    Types = new[] {
                    typeof(Migration1)
                }
                });

                Assert.NotNull(store.DatabaseCommands.GetIndex("BooksByTitle"));
            
                migrator.Migrate(store, new FakeAssembly { Types = new[] { typeof(Migration1) } }, 0);

                Assert.Null(store.DatabaseCommands.GetIndex("BooksByTitle"));
            }
        }

        Type[] MigrationTypes()
        {
            return new[] {
                typeof(Migration1)
            };
        }

    }

    class FakeAssembly : Assembly
    {
        public Type[] Types { get; set; }

        public override Type[] GetExportedTypes()
        {
            return Types;
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
        public void Up(IDocumentStore store)
        {
            store.DatabaseCommands.PutIndex<Book, Book>("BooksByTitle", new Client.Indexes.IndexDefinition<Book, Book>
            {
                Map = books => from b in books
                               select new { b.Title }
            });
        }

        public void Down(IDocumentStore store)
        {
            store.DatabaseCommands.DeleteIndex("BooksByTitle");
        }
    }

    [Migration(2)]
    public class Migration2 : IMigration
    {
        public void Up(IDocumentStore store)
        {
            store.DatabaseCommands.PutIndex<Book, Book>("BooksByAuthor", new Client.Indexes.IndexDefinition<Book, Book>
            {
                Map = books => from b in books
                               select new { b.Author }
            });
        }

        public void Down(IDocumentStore store)
        {
            store.DatabaseCommands.DeleteIndex("BooksByAuthor");
        }
    }

}
