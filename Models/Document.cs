using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace myazfunction.Models
{
    public class Document
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string[] Tags { get; set; }

        public byte[] File { get; set; } // This stores the document file

        public string FileName { get; set; }
    }

}
