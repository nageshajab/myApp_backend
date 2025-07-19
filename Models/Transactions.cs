using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace myazfunction.Models
{
    public class Transactions
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title{ get; set; }

        public DateTime Date{ get; set; }

        public string Amount{ get; set; }

        public string UserId { get; set; }

        public string Description { get; set; }
    }

}
