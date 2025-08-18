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
        NotStarted=1,
        InProgress=2,
        Completed=3,
        Cancelled=4
    }
    public enum WatchlistType
    {
        Movie=1,
        WebSeries=2,
        Documentary=3,
        Other=4
    }
    public enum Language
    {
        English=1,
        Hindi=2,
        Tamil=3,        
        Malyalam=4,
        Marathi=5,
        Korean=6,
        Other=7
    }
    public enum Genre
    {
        Action=1,
        Comedy=2,
        Drama=3,
        Horror=4,
        Thriller=5,
        Romance=6,
        SciFi=7,
        Documentary=8,
        Other=9
    }
    public enum Rating
    {
        OneStar=1,
        TwoStars=2,
        ThreeStars=3,
        FourStars=4,
        FiveStars=5
    }
    public enum Ott
    {
        Netflix=1,
        Prime=2,
        Hotstar=3,
        SonyLiv=4,
        Zee5=5,
        YouTube=6,
        Other=7
    }
}