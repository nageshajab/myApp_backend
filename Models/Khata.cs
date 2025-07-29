using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace myazfunction.Models
{
    public class Khata
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title{ get; set; }

        public DateTime Date{ get; set; }

        public int Amount{ get; set; }

        public string UserId { get; set; }

        public string PersonName { get; set; }
    }

}
