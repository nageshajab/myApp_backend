using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace myazfunction.DAL
{
    public class DatesRepository
    {
        private readonly IMongoCollection<Dates> _dates;

        public DatesRepository(MongoDbContext context)
        {
            _dates= context.GetCollection<Dates>("dates");
        }

        public async Task<OkObjectResult> GetAllDatesAsync(string userid,string searchtxt,int pageNumber)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Dates>.Filter;
            var filter = builder.Eq(p => p.userid, userid);

            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.Title, new BsonRegularExpression(searchtxt, "i"));
                filter = builder.And(filter, searchFilter);
            }

            // Get total count for pagination
            var totalCount = await _dates.CountDocumentsAsync(filter);

            // Apply pagination
            var documents = await _dates
                .Find(filter)
                .Skip((pageNumber - 1) * 10)
                .Limit(10)
                .ToListAsync();

            List<Dates> reurnval= new List<Dates>();
            foreach(Dates dt in documents)
            {
                Dates date = new Dates();
                date.Id = dt.Id;
                date.Title = dt.Title;
                date.Description = dt.Description;
                date.Date = dt.Date;
                date.Duration = Dates.CalculateDuration(DateTime.Parse(dt.Date));
                date.isRecurring = dt.isRecurring;
                if (dt.RecurringEvent != null)
                {
                    date.RecurringEvent = new RecurringEvent
                    {
                        Frequency = dt.RecurringEvent.Frequency
                    };
                }
                reurnval.Add(date);
            }
            var result = new
            {
                dates = reurnval,
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

        public async Task<Dates> GetDateAsync(string id)
        {
            return await _dates.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task CreateDateAsync(Dates dates)
        {            
            await _dates.InsertOneAsync(dates);
        }

        public async System.Threading.Tasks.Task UpdateDateAsync(string id, Dates dates)
        {
            await _dates.ReplaceOneAsync(d=> d.Id == id, dates);
        }

        public async System.Threading.Tasks.Task DeleteDateAsync(string id)
        {
            await _dates.DeleteOneAsync(d => d.Id == id);
        }
    }
}
