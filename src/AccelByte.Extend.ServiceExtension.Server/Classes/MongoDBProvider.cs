// Copyright (c) 2026 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using MongoDB.Driver;
using AccelByte.Extend.ServiceExtension.Server.Model;

namespace AccelByte.Extend.ServiceExtension.Server
{
    public class MongoDBProvider : IMongoDBProvider
    {
        private readonly ILogger<MongoDBProvider> _Logger;

        private string _ConnectionString;

        private string _DatabaseName;

        private MongoClient _Client;

        protected string GetVariable(string name, bool required)
        {
            string? value = Environment.GetEnvironmentVariable(name);
            if ((value != null) && (value.Trim() != ""))
                value = value.Trim();
            if (required && value == null)
            {
                _Logger.LogError($"Missing {name} environment variable.");
                throw new ArgumentNullException($"Missing {name} environment variable.");
            }
            return value != null ? value : "";
        }

        public MongoDBProvider(ILogger<MongoDBProvider> logger)
        {
            _Logger = logger;

            string host = GetVariable("DOCDB_HOST", true);
            string username = GetVariable("DOCDB_USERNAME", true);
            string password = GetVariable("DOCDB_PASSWORD", true);
            _DatabaseName = GetVariable("DOCDB_DATABASE_NAME", true);
            string caFile = GetVariable("DOCDB_CA_CERT_FILE_PATH", false);

            if (username != "" && password != "")
            {
                if (caFile != "")
                {
                    _ConnectionString = $"mongodb://{username}:{password}@{host}/?tls=true&tlsCAFile={caFile}";
                    _Logger.LogInformation("MongoDB connection using credential and TLS.");
                }

                else
                {
                    _ConnectionString = $"mongodb://{username}:{password}@{host}";
                    _Logger.LogInformation("MongoDB connection using credential only.");
                }

            }
            else
            {
                _ConnectionString = $"mongodb://{host}";
                _Logger.LogInformation("MongoDB connection without credential.");
            }

            _Client = new MongoClient(_ConnectionString);

            //index initialization for certain collection
            var database = _Client.GetDatabase(_DatabaseName);            

            var indexKeys = Builders<GuildProgressData>.IndexKeys
                .Ascending(x => x.Namespace)
                .Ascending(x => x.Key);
            List<CreateIndexModel<GuildProgressData>> indexModels =
                [new CreateIndexModel<GuildProgressData>(indexKeys, new CreateIndexOptions() { Unique = true })];

            var collection = database.GetCollection<GuildProgressData>("guild_progress");
            collection.Indexes.CreateMany(indexModels);
        }

        public IMongoClient GetClient()
            => _Client;

        public IMongoDatabase GetDatabase()
            => _Client.GetDatabase(_DatabaseName);

        public IMongoCollection<GuildProgressData> GetGuildProgressCollection()
            => _Client.GetDatabase(_DatabaseName).GetCollection<GuildProgressData>("guild_progress");
    }
}
