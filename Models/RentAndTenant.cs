using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace myazfunction.Models
{
    public class Rent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime Date{ get; set; }

        public int PaidAmount{ get; set; }

        public int RemainingAmount { get; set; }

        public int Mseb { get; set; }

        public string UserId { get; set; }

        public string TenantName { get; set; }
    }

    public class Tenant
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int Deposit{ get; set; }

        public string UserId { get; set; }

        public string TenantName { get; set; }
    }
}
