using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace myazfunction.Models
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public bool Isverified { get; set; }
    }

    public class ChangePassword
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string currentPassword { get; set; }

        public string password { get; set; }
    }
}
