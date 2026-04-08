// Copyright (c) 2023-2026 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AccelByte.Sdk.Api.Cloudsave.Model;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace AccelByte.Extend.ServiceExtension.Server.Model
{
    public class GuildProgressData : ModelsGameRecordRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string? Id { get; set; } = "";

        [BsonElement("key")]
        [JsonPropertyName("key")]
        public string Key { get; set; } = "";

        [BsonElement("guild_id")]
        [JsonPropertyName("guild_id")]
        public string GuildId { get; set; } = "";

        [BsonElement("namespace")]
        [JsonPropertyName("namespace")]        
        public string Namespace { get; set; } = "";

        [BsonElement("objectives")]
        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        [JsonPropertyName("objectives")]
        public Dictionary<string, int> Objectives { get; set; } = new();

        [BsonElement("created_at")]
        [BsonRepresentation(BsonType.String)]
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updated_at")]
        [BsonRepresentation(BsonType.String)]
        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; } = null;

        public static GuildProgressData FromGuildProgressGrpcData(GuildProgress src, string key)
        {
            return new GuildProgressData()
            {
                Key = key,
                GuildId = src.GuildId,
                Namespace = src.Namespace,
                Objectives = new Dictionary<string, int>(src.Objectives),
                CreatedAt = DateTime.Now
            };
        }

        public GuildProgress ToGuildProgressGrpcData()
        {
            GuildProgress data = new GuildProgress()
            {
                GuildId = GuildId,
                Namespace = Namespace
            };

            data.Objectives.Add(Objectives);
            return data;
        }
    }
}
