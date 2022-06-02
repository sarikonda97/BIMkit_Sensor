using DbmsApi.API;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DbmsApi.Mongo
{
    public class MongoDeviceRelationships : MongoDocument
    {
        public string Subject;
        public string Predicate;
        public string Object;
    }
}