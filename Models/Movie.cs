using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace myazfunction.Models
{
    public class Movie
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }

        public string Title { get; set; }
        public string tags { get; set; }
        public string Url { get; set; }
        public byte[] ImageData { get; set; } // This stores the image
    }

    public class MovieTags
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Tag { get; set; } 
    }
}