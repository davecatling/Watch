using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace WatchFunctions
{
    public static class Entities
    {
        public static async Task<TableResult> SaveEntityAsync(string tableName, TableEntity newEntity)
        {
            var insertOperation = TableOperation.Insert(newEntity);
            var table = Table(tableName);
            return await table.ExecuteAsync(insertOperation);
        }

        public static async Task<TableResult> UpdateEntityAsync(string tableName, TableEntity entity)
        {
            var updateOperation = TableOperation.InsertOrMerge(entity);
            var table = Table(tableName);
            return await table.ExecuteAsync(updateOperation);
        }

        public static async Task<TEntity> GetEntityAsync<TEntity>(string tableName, string partitionKey, string rowkey) where TEntity : TableEntity, new()
        {
            var table = Table(tableName);
            var result = await table.ExecuteAsync(TableOperation.Retrieve<TEntity>(partitionKey, rowkey));
            return (TEntity)result.Result;
        }

        public static CloudTable Table(string tableName)
        {
            var connString = Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING", EnvironmentVariableTarget.Process);
            //var connString = @"AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
            var storageAccount = CloudStorageAccount.Parse(connString);
            var tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference(tableName);
        }
    }
}
