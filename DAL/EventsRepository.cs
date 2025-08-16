using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class EventsRepository
    {
        private readonly IMongoCollection<Events> _Events;

        public EventsRepository(MongoDbContext context)
        {
            _Events = context.GetCollection<Events>("Events");
        }

        public async Task<ReturnValEvents> GetAllEventsAsync(string userid, string searchtxt, int pageNumber, bool showall,int pageSize=10)
        {
            var currentDate = DateTime.Now;
            var fiveDaysAgo = currentDate.AddDays(-5);
            var fiveDaysLater = currentDate.AddDays(5);

            // Build filter
            var builder = Builders<Events>.Filter;
            var filter = builder.Eq(p => p.userid, userid);
            if (!showall)
            {
                filter = builder.And(filter, builder.Eq(p => p.MarkFinished, false));// Only get events that are not marked as finished

                filter = builder.And(filter, builder.Gte(p => p.Date, fiveDaysAgo));
                filter = builder.And(filter, builder.Lte(p => p.Date, fiveDaysLater));
            }
            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.Title, new BsonRegularExpression(searchtxt, "i"));
                filter = builder.And(filter, searchFilter);
            }

            // Get total count for pagination
            var totalCount = await _Events.CountDocumentsAsync(filter);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Apply pagination
            var documents = await _Events
                .Find(filter)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            List<Events> returnVal = new List<Events>();
            foreach (Events dt in documents)
            {
                Events Event = new Events();
                Event.Id = dt.Id;
                Event.Title = dt.Title;
                Event.Description = dt.Description;
                Event.Date = dt.Date;
                Event.MarkFinished = dt.MarkFinished;
                Event.Duration = Dates.CalculateDuration(dt.Date);
                returnVal.Add(Event);
            }

            var result = new ReturnValEvents()
            {
                Events = returnVal,
                Pagination = new Pagination()
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                }
            };

            return result;
        }

        public async Task<Events> GetEventAsync(string id)
        {
            return await _Events.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task CreateEventAsync(Events Events)
        {
            await _Events.InsertOneAsync(Events);
        }

        public async System.Threading.Tasks.Task UpdateEventAsync(string id, Events Events)
        {
            await _Events.ReplaceOneAsync(d => d.Id == id, Events);
        }

        public async System.Threading.Tasks.Task DeleteEventAsync(string id)
        {
            await _Events.DeleteOneAsync(d => d.Id == id);
        }
    }
    public class ReturnValEvents
    {
        public List<Events> Events { get; set; }
        public Pagination Pagination { get; set; }
    }
    public class Pagination
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}