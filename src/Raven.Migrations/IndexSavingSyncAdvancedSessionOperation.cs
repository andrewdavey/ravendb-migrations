using System;
using Newtonsoft.Json.Linq;
using Raven.Client;
using Raven.Client.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace Raven.Migrations
{
    /// <summary>
    /// Wraps an ISyncAdvancedSessionOperation to provide a wrapped DatabaseCommands property.
    /// </summary>
    class IndexSavingSyncAdvancedSessionOperation : ISyncAdvancedSessionOperation
    {
        public IndexSavingSyncAdvancedSessionOperation(ISyncAdvancedSessionOperation inner)
        {
            this.inner = inner;
        }

        readonly ISyncAdvancedSessionOperation inner;
        IndexSavingDatabaseCommands databaseCommands;

        public void RestoreIndexes()
        {
            databaseCommands.RestoreIndexes();
        }

        public IDatabaseCommands DatabaseCommands
        {
            get
            {
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

        public IDocumentQuery<T> LuceneQuery<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
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

        public DocumentConvention Conventions
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

        public JObject GetMetadataFor<T>(T instance)
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
    }
}
