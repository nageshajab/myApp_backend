using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace myazfunction.Models
{
    public class Dates
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Date { get; set; }

        public string Duration { get; set; }

        public bool isRecurring { get; set; }

        public RecurringEvent RecurringEvent { get; set; }

        public string userid { get; set; }
    }
    
    public class RecurringEvent
    {
        public Frequency Frequency { get; set; }        
    }

    public enum Frequency
    {
        Daily,
        Weekly,
        Monthly,
        Custom
    }
}
