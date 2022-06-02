using DbmsApi.API;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DbmsApi.Mongo
{
    public class MongoDeviceObject : MongoDocument
    {
        public string Name;
        public string TypeId;

        public Properties Properties;
        public List<KeyValuePair<string, string>> Tags = new List<KeyValuePair<string, string>>();
    }

    public class DeviceReference : BaseObject
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string DeviceId;
    }
}