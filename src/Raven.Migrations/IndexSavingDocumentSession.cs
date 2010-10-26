using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using Raven.Client.Client;

namespace Raven.Migrations
{
    class IndexSavingDocumentSession : IDocumentSession
    {
        readonly IDocumentSession inner;
        CustomSyncAdvancedSessionOperation advanced;

        public IndexSavingDocumentSession(IDocumentSession inner)
        {
            this.inner = inner;
        }

        public void RestoreIndexes()
        {
            advanced.RestoreIndexes();
        }

        public ISyncAdvancedSessionOperation Advanced
        {
            get
            {
                if (advanced == null) advanced = new CustomSyncAdvancedSessionOperation(inner.Advanced);
                return advanced;
            }
        }

        public void Delete<T>(T entity)
        {
            inner.Delete(entity);
        }

        public Client.Document.ILoaderWithInclude<T> Include<T>(System.Linq.Expressions.Expression<Func<T, object>> path)
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

        public Client.Linq.IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : Client.Indexes.AbstractIndexCreationTask, new()
        {
            return inner.Query<T, TIndexCreator>();
        }

        public Client.Linq.IRavenQueryable<T> Query<T>()
        {
            return inner.Query<T>();
        }

        public Client.Linq.IRavenQueryable<T> Query<T>(string indexName)
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

    class CustomSyncAdvancedSessionOperation : ISyncAdvancedSessionOperation
    {
        public CustomSyncAdvancedSessionOperation(ISyncAdvancedSessionOperation inner)
        {
            this.inner = inner;
        }

        readonly ISyncAdvancedSessionOperation inner;
        IndexSavingDatabaseCommands databaseCommands;

        public IDatabaseCommands DatabaseCommands
        {
            get {
                if (databaseCommands == null) databaseCommands = new IndexSavingDatabaseCommands(inner.DatabaseCommands);
                return databaseCommands;
            }
        }

        public IDocumentQuery<T> DynamicLuceneQuery<T>()
        {
            return inner.DynamicLuceneQuery<T>();
        }

        public string GetDocumentUrl(object entity)
        {
            return inner.GetDocumentUrl(entity);
        }

        public IDocumentQuery<T> LuceneQuery<T>(string indexName)
        {
            return inner.LuceneQuery<T>(indexName);
        }

        public IDocumentQuery<T> LuceneQuery<T, TIndexCreator>() where TIndexCreator : Client.Indexes.AbstractIndexCreationTask, new()
        {
            return inner.LuceneQuery<T, TIndexCreator>();
        }

        public void Refresh<T>(T entity)
        {
            inner.Refresh<T>(entity);
        }

        public bool AllowNonAuthoritiveInformation
        {
            get
            {
                return inner.AllowNonAuthoritiveInformation;
            }
            set
            {
                inner.AllowNonAuthoritiveInformation = value;
            }
        }

        public void Clear()
        {
            inner.Clear();
        }

        public Client.Document.DocumentConvention Conventions
        {
            get { return inner.Conventions; }
        }

        public void Evict<T>(T entity)
        {
            inner.Evict(entity);
        }

        public string GetDocumentId(object entity)
        {
            return inner.GetDocumentId(entity);
        }

        public Newtonsoft.Json.Linq.JObject GetMetadataFor<T>(T instance)
        {
            return inner.GetMetadataFor(instance);
        }

        public bool HasChanged(object entity)
        {
            return inner.HasChanged(entity);
        }

        public bool HasChanges
        {
            get { return inner.HasChanges; }
        }

        public int MaxNumberOfRequestsPerSession
        {
            get
            {
                return inner.MaxNumberOfRequestsPerSession;
            }
            set
            {
                inner.MaxNumberOfRequestsPerSession = value;
            }
        }

        public TimeSpan NonAuthoritiveInformationTimeout
        {
            get
            {
                return inner.NonAuthoritiveInformationTimeout;
            }
            set
            {
                inner.NonAuthoritiveInformationTimeout = value;
            }
        }

        public int NumberOfRequests
        {
            get { return inner.NumberOfRequests; }
        }

        public event EntityToDocument OnEntityConverted
        {
            add { inner.OnEntityConverted += value; }
            remove { inner.OnEntityConverted -= value; }
        }

        public string StoreIdentifier
        {
            get { return inner.StoreIdentifier; }
        }

        public event EntityStored Stored
        {
            add { inner.Stored += value; }
            remove { inner.Stored -= value; }
        }

        public bool UseOptimisticConcurrency
        {
            get
            {
                return inner.UseOptimisticConcurrency;
            }
            set
            {
                inner.UseOptimisticConcurrency = value;
            }
        }

        public void RestoreIndexes()
        {
            databaseCommands.RestoreIndexes();
        }
    }
}
