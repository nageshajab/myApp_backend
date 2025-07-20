using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace myazfunction.Models
{
    public class Watchlist
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public Status Status { get; set; }  
        public string UserId { get; set; }
        public WatchlistType Type { get; set; }
        public Language Language { get; set; }
        public Genre Genre { get; set; }
        public Rating Rating { get; set; }
        public Ott Ott { get; set; }
    }
    public enum Status
    {
        NotStarted,
        InProgress,
        Completed,
        Cancelled
    }
    public enum WatchlistType
    {
        Movie,
        WebSeries,
        Documentary,
        Other
    }
    public enum Language
    {
        English,
        Hindi,
        Tamil,        
        Malyalam,
        Marathi,
        Other
    }
    public enum Genre
    {
        Action,
        Comedy,
        Drama,
        Horror,
        Thriller,
        Romance,
        SciFi,
        Documentary,
        Other
    }
    public enum Rating
    {
        OneStar,
        TwoStars,
        ThreeStars,
        FourStars,
        FiveStars
    }
    public enum Ott
    {
        Netflix,
        Prime,
        Hotstar,
        SonyLiv,
        Zee5,
        YouTube,
        Other
    }
}