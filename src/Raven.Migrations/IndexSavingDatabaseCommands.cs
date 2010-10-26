using System;
using System.Collections.Generic;
using Raven.Client.Client;
using Raven.Database.Indexing;

namespace Raven.Migrations
{
    /// <summary>
    /// Wraps an IDatabaseCommands implementation and saves existing indexes before changing/deleting them.
    /// </summary>
    class IndexSavingDatabaseCommands : IDatabaseCommands
    {
        IDatabaseCommands inner;
        Dictionary<string, IndexDefinition> savedIndexes;

        public IndexSavingDatabaseCommands(IDatabaseCommands inner)
        {
            this.inner = inner;
            savedIndexes = new Dictionary<string, IndexDefinition>();
        }

        void SaveUnseenIndex(string name)
        {
            if (!savedIndexes.ContainsKey(name))
            {
                savedIndexes[name] = inner.GetIndex(name);
            }
        }

        public void RestoreIndexes()
        {
            foreach (var item in savedIndexes)
            {
                if (item.Value == null)
                {
                    // index did not exist before, so delete it
                    inner.DeleteIndex(item.Key);
                }
                else
                {
                    inner.PutIndex(item.Key, item.Value, true);
                }
            }
        }

        public Database.BatchResult[] Batch(Database.Data.ICommandData[] commandDatas)
        {
            return inner.Batch(commandDatas);
        }

        public void Commit(Guid txId)
        {
            inner.Commit(txId);
        }

        public void Delete(string key, Guid? etag)
        {
            inner.Delete(key, etag);
        }

        public void DeleteAttachment(string key, Guid? etag)
        {
            inner.DeleteAttachment(key, etag);
        }

        public void DeleteByIndex(string indexName, Database.Data.IndexQuery queryToDelete, bool allowStale)
        {
            inner.DeleteByIndex(indexName, queryToDelete, allowStale);
        }

        public void DeleteIndex(string name)
        {
            SaveUnseenIndex(name);
            inner.DeleteIndex(name);
        }

        public IDatabaseCommands ForDatabase(string database)
        {
            return inner.ForDatabase(database);
        }

        public Database.Data.MultiLoadResult Get(string[] ids, string[] includes)
        {
            return inner.Get(ids, includes);
        }

        public Database.JsonDocument Get(string key)
        {
            return inner.Get(key);
        }

        public Database.Data.Attachment GetAttachment(string key)
        {
            return inner.GetAttachment(key);
        }

        public Database.Indexing.IndexDefinition GetIndex(string name)
        {
            return inner.GetIndex(name);
        }

        public string[] GetIndexNames(int start, int pageSize)
        {
            return inner.GetIndexNames(start, pageSize);
        }

        public System.Collections.Specialized.NameValueCollection OperationsHeaders
        {
            get
            {
                return inner.OperationsHeaders;
            }
            set
            {
                inner.OperationsHeaders = value;
            }
        }

        public byte[] PromoteTransaction(Guid fromTxId)
        {
            return inner.PromoteTransaction(fromTxId);
        }

        public Database.PutResult Put(string key, Guid? etag, Newtonsoft.Json.Linq.JObject document, Newtonsoft.Json.Linq.JObject metadata)
        {
            return inner.Put(key, etag, document, metadata);
        }

        public void PutAttachment(string key, Guid? etag, byte[] data, Newtonsoft.Json.Linq.JObject metadata)
        {
            inner.PutAttachment(key, etag, data, metadata);
        }

        public string PutIndex<TDocument, TReduceResult>(string name, Client.Indexes.IndexDefinition<TDocument, TReduceResult> indexDef, bool overwrite)
        {
            SaveUnseenIndex(name);
            return inner.PutIndex<TDocument, TReduceResult>(name, indexDef, overwrite);
        }

        public string PutIndex(string name, Database.Indexing.IndexDefinition indexDef, bool overwrite)
        {
            SaveUnseenIndex(name);
            return inner.PutIndex(name, indexDef, overwrite);
        }

        public string PutIndex<TDocument, TReduceResult>(string name, Client.Indexes.IndexDefinition<TDocument, TReduceResult> indexDef)
        {
            SaveUnseenIndex(name);
            return inner.PutIndex<TDocument, TReduceResult>(name, indexDef);
        }

        public string PutIndex(string name, Database.Indexing.IndexDefinition indexDef)
        {
            SaveUnseenIndex(name);
            return inner.PutIndex(name, indexDef);
        }

        public Database.Data.QueryResult Query(string index, Database.Data.IndexQuery query, string[] includes)
        {
            return inner.Query(index, query, includes);
        }

        public void ResetIndex(string name)
        {
            inner.ResetIndex(name);
        }

        public void Rollback(Guid txId)
        {
            inner.Rollback(txId);
        }

        public void StoreRecoveryInformation(Guid resourceManagerId, Guid txId, byte[] recoveryInformation)
        {
            inner.StoreRecoveryInformation(resourceManagerId, txId, recoveryInformation);
        }

        public bool SupportsPromotableTransactions
        {
            get { return inner.SupportsPromotableTransactions; }
        }

        public void UpdateByIndex(string indexName, Database.Data.IndexQuery queryToUpdate, Database.Json.PatchRequest[] patchRequests, bool allowStale)
        {
            inner.UpdateByIndex(indexName, queryToUpdate, patchRequests, allowStale);
        }

        public IDatabaseCommands With(System.Net.ICredentials credentialsForSession)
        {
            return inner.With(credentialsForSession);
        }
    }
}
