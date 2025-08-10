using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace myazfunction.Models
{
    public class Dates
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }

        [NotMapped]
        public string Duration { get; set; }

        public bool isRecurring { get; set; }

        public RecurringEvent RecurringEvent { get; set; }

        public string userid { get; set; }

        public static string CalculateDuration(DateTime pastDate)
        {
            DateTime now = DateTime.Now;
            int years = 0;
            int months = 0;
            int days = 0;

            // Calculate years
            while (pastDate.AddYears(1) <= now)
            {
                pastDate = pastDate.AddYears(1);
                years++;
            }

            // Calculate months
            while (pastDate.AddMonths(1) <= now)
            {
                pastDate = pastDate.AddMonths(1);
                months++;
            }

            // Calculate days
            days = (now - pastDate).Days;

            return $"{years} years, {months} months, {days} days";
        }
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
