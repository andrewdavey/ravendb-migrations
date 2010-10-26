using System;
using System.Linq.Expressions;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Client.Linq;

namespace Raven.Migrations
{
    /// <summary>
    /// Wraps an IDocumentSession to provide a wrapped Advanced property.
    /// </summary>
    class IndexSavingDocumentSession : IDocumentSession
    {
        public IndexSavingDocumentSession(IDocumentSession inner)
        {
            this.inner = inner;
        }

        readonly IDocumentSession inner;
        IndexSavingSyncAdvancedSessionOperation advanced;

        public void RestoreIndexes()
        {
            advanced.RestoreIndexes();
        }

        public ISyncAdvancedSessionOperation Advanced
        {
            get
            {
                if (advanced == null) advanced = new IndexSavingSyncAdvancedSessionOperation(inner.Advanced);
                return advanced;
            }
        }

        public void Delete<T>(T entity)
        {
            inner.Delete(entity);
        }

        public Client.Document.ILoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            return inner.Include<T>(path);
        }

        public Client.Document.ILoaderWithInclude<object> Include(string path)
        {
            return inner.Include(path);
        }

        public T[] Load<T>(params string[] ids)
        {
            return inner.Load<T>(ids);
        }

        public T Load<T>(string id)
        {
            return inner.Load<T>(id);
        }

        public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
        {
            return inner.Query<T, TIndexCreator>();
        }

        public IRavenQueryable<T> Query<T>()
        {
            return inner.Query<T>();
        }

        public IRavenQueryable<T> Query<T>(string indexName)
        {
            return inner.Query<T>(indexName);
        }

        public void SaveChanges()
        {
            inner.SaveChanges();
        }

        public void Store(dynamic entity)
        {
            inner.Store(entity);
        }

        public void Dispose()
        {
            inner.Dispose();
        }
    }

}
