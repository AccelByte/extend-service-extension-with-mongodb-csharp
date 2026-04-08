// Copyright (c) 2023-2026 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Grpc.Core;
using AccelByte.Sdk.Api;
using AccelByte.Extend.ServiceExtension.Server.Model;
using MongoDB.Driver;

namespace AccelByte.Extend.ServiceExtension.Server.Services
{
    public class MyService : Service.ServiceBase
    {
        private readonly ILogger<MyService> _Logger;

        private readonly IAccelByteServiceProvider _ABProvider;

        private readonly IMongoDBProvider _DBProvider;

        public MyService(
            ILogger<MyService> logger,
            IAccelByteServiceProvider abProvider,
            IMongoDBProvider dbProvider)
        {
            _Logger = logger;
            _ABProvider = abProvider;
            _DBProvider = dbProvider;
        }

        public override async Task<CreateOrUpdateGuildProgressResponse> CreateOrUpdateGuildProgress(CreateOrUpdateGuildProgressRequest request, ServerCallContext context)
        {
            string actualGuildId = request.GuildProgress.GuildId.Trim();
            if (actualGuildId == "")
                actualGuildId = Guid.NewGuid().ToString().Replace("-", "");

            string gpKey = $"guildProgress_{actualGuildId}";
            var gpValue = GuildProgressData.FromGuildProgressGrpcData(request.GuildProgress, gpKey);
            gpValue.GuildId = actualGuildId;            

            var filter = Builders<GuildProgressData>.Filter.And(
                Builders<GuildProgressData>.Filter.Eq(x => x.Namespace, request.Namespace),
                Builders<GuildProgressData>.Filter.Eq(x => x.Key, gpKey)
            );
            var update = Builders<GuildProgressData>.Update
                .Set(x => x.GuildId, gpValue.GuildId)
                .Set(x => x.Objectives, gpValue.Objectives)
                .Set(x => x.UpdatedAt, DateTime.Now)
                .SetOnInsert(x => x.CreatedAt, DateTime.Now);
            var options = new UpdateOptions() { IsUpsert = true };

            var collection = _DBProvider.GetGuildProgressCollection();
            await collection.UpdateOneAsync(filter, update, options);

            return await Task.FromResult(new CreateOrUpdateGuildProgressResponse()
            {
                GuildProgress = gpValue.ToGuildProgressGrpcData()
            });
        }

        public override async Task<GetGuildProgressResponse> GetGuildProgress(GetGuildProgressRequest request, ServerCallContext context)
        {
            string gpKey = $"guildProgress_{request.GuildId.Trim()}";

            var filter = Builders<GuildProgressData>.Filter.And(
                Builders<GuildProgressData>.Filter.Eq(x => x.Namespace, request.Namespace),
                Builders<GuildProgressData>.Filter.Eq(x => x.Key, gpKey)
            );

            var collection = _DBProvider.GetGuildProgressCollection();
            var savedData = await collection
                .Find(filter)
                .FirstOrDefaultAsync();

            if (savedData == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Guild progress not found"));

            return await Task.FromResult(new GetGuildProgressResponse()
            {
                GuildProgress = savedData.ToGuildProgressGrpcData()
            });
        }
    }
}
