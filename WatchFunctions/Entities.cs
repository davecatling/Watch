using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchFunctions.Model;

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

        public static async Task<UserEntity> GetUserBySessionAsync(string session)
        {
            var table = Table("users");
            TableQuery<UserEntity> userQuery = new TableQuery<UserEntity>().Where($"SessionToken eq '{session}'");
            var result = await table.ExecuteQuerySegmentedAsync(userQuery, null);
            if (result.Results.Count > 0)
            {
                var user = (UserEntity)result.Results[0];
                var lastAccessed = DateTime.Parse(user.LastAccess);
                if (DateTime.Now.Subtract(lastAccessed) > TimeSpan.FromMinutes(1))
                    throw new InvalidOperationException("Session expired");
                user.LastAccess = DateTime.Now.ToString();
                _ = await UpdateEntityAsync("users", user);
                return user;
            }
            return null;
        }

        public static async Task<IEnumerable<MessageEntity>> ReadMessages(string channelNumber)
        {
            var table = Table("messages");
            TableQuery<MessageEntity> messageQuery = new TableQuery<MessageEntity>().Where($"PartitionKey " +
                $"eq '{channelNumber}'");
            var result = await table.ExecuteQuerySegmentedAsync(messageQuery, null);
            return result.Results;
        }

        public static async Task<bool> HasAccess(string channelNumber, string handle)
        {
            var table = Table("access");
            TableQuery<AccessEntity> accessQuery = new TableQuery<AccessEntity>().Where($"PartitionKey " +
                $"eq '{channelNumber}'");
            var result = await table.ExecuteQuerySegmentedAsync(accessQuery, null);
            if (result.Results.Count == 0)
            {
                var accessEntity = new AccessEntity()
                {
                    PartitionKey = channelNumber,
                    RowKey = Guid.NewGuid().ToString(),
                    Handle = handle,
                    Grantor = "SYSTEM"
                };
                _ = await SaveEntityAsync("access", accessEntity);
            }
            accessQuery = new TableQuery<AccessEntity>().Where($"PartitionKey " +
                $"eq '{channelNumber}' and Handle eq '{handle}'");
            result = await table.ExecuteQuerySegmentedAsync(accessQuery, null);
            return result.Results.Count != 0;
        }

        public static async Task<List<string>> HandlesWithAccess(string channelNumber)
        {
            var result = new List<string>();
            var table = Table("access");
            TableQuery<AccessEntity> accessQuery = new TableQuery<AccessEntity>().Where($"PartitionKey " +
                $"eq '{channelNumber}'");
            var query = await table.ExecuteQuerySegmentedAsync(accessQuery, null);
            if (query.Results.Count == 0)
            {
                return result;
            }
            query.Results.ForEach(access => result.Add(access.Handle));
            return result;
        }

        public static CloudTable Table(string tableName)
        {
            //var connString = Environment.GetEnvironmentVariable("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING", EnvironmentVariableTarget.Process);
            var connString = @"AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
            var storageAccount = CloudStorageAccount.Parse(connString);
            var tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference(tableName);
        }
    }
}
