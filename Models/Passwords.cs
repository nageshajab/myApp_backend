using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace myazfunction.Models
{
    public class Passwords
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string System{ get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string UserId { get; set; }
    }

}
