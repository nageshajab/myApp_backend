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
            _Events= context.GetCollection<Events>("Events");
        }

        public async Task<List<Events>> GetAllEventsAsync(string userid)
        {
            // Build filter
            var builder = Builders<Events>.Filter;
            var filter = builder.Eq(p => p.userid, userid);

            // Apply pagination
            var documents = await _Events
                .Find(filter)
                .ToListAsync();

            List<Events> reurnval = new List<Events>();
            foreach (Events dt in documents)
            {
                Events Event = new Events();
                Event.Id = dt.Id;
                Event.Title = dt.Title;
                Event.Description = dt.Description;
                Event.Date = dt.Date;
                Event.MarkFinished= dt.MarkFinished;
                reurnval.Add(Event);
            }
            
            return reurnval;
        }

        public async Task<OkObjectResult> GetAllEventsAsync(string userid,string searchtxt,int pageNumber)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Events>.Filter;
            var filter = builder.Eq(p => p.userid, userid);
            filter = builder.And(filter, builder.Eq(p => p.MarkFinished, false));// Only get events that are not marked as finished
            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.Title, new BsonRegularExpression(searchtxt, "i"));
                filter = builder.And(filter, searchFilter);
            }

            // Get total count for pagination
            var totalCount = await _Events.CountDocumentsAsync(filter);

            // Apply pagination
            var documents = await _Events
                .Find(filter)
                .Skip((pageNumber - 1) * 10)
                .Limit(10)
                .ToListAsync();

            List<Events> reurnval= new List<Events>();
            foreach(Events dt in documents)
            {
                Events Event = new Events();
                Event.Id = dt.Id;
                Event.Title = dt.Title;
                Event.Description = dt.Description;
                Event.Date = dt.Date;
               Event.MarkFinished = dt.MarkFinished;
                reurnval.Add(Event);
            }
            var result = new
            {
                Events = reurnval,
                pagination = new
                {
                    pageNumber,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            };

            return new OkObjectResult(result);
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
            await _Events.ReplaceOneAsync(d=> d.Id == id, Events);
        }

        public async System.Threading.Tasks.Task DeleteEventAsync(string id)
        {
            await _Events.DeleteOneAsync(d => d.Id == id);
        }
    }
}
