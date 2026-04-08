// Copyright (c) 2026 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Extend.ServiceExtension.Server.Model;
using MongoDB.Driver;

namespace AccelByte.Extend.ServiceExtension.Server
{
    public interface IMongoDBProvider
    {
        IMongoClient GetClient();

        IMongoDatabase GetDatabase();

        IMongoCollection<GuildProgressData> GetGuildProgressCollection();
    }
}
