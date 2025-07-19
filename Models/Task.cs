using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace myazfunction.Models
{
    public class Task
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public TaskStatus Status { get; set; }

        public string UserId { get; set; }
    }

    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Cancelled
    }
}
