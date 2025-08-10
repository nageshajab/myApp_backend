using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class TaskRepository
    {
        private readonly IMongoCollection<Models.Task> _entries;

        public TaskRepository(MongoDbContext context)
        {
            _entries = context.GetCollection<Models.Task>("tasks");
        }

        public async Task<List<Models.Task>> GetAllTasksEntriesAsync(string userid)
        {
            // Build filter
            var builder = Builders<Models.Task>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);
                        
            // Apply pagination
            var documents = await _entries
                .Find(filter)
                .ToListAsync();       

            return documents;
        }

        public async Task<OkObjectResult> GetAllTasksEntriesAsync(string userid, string searchtxt, int pageNumber)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Models.Task>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.Title, new BsonRegularExpression(searchtxt, "i"));
                filter = builder.And(filter, searchFilter);
            }
           
            // Get total count for pagination
            var totalCount = await _entries.CountDocumentsAsync(filter);

            // Apply pagination
            var documents = await _entries
                .Find(filter)
                .Skip((pageNumber - 1) * 10)
                .Limit(10)
                .ToListAsync();

            List<Models.Task> reurnval = new List<Models.Task>();
            foreach (Models.Task dt in documents)
            {
                Models.Task khata= new Models.Task();
                khata.Id = dt.Id;
                khata.Title = dt.Title;
                khata.Status= dt.Status;
                khata.Date = dt.Date;
                khata.UserId=dt.UserId;
                khata.Description= dt.Description;

                reurnval.Add(khata);
            }
        
            var result = new
            {
                TaskEntries = reurnval,
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

        public async Task<Models.Task> GetTaskEntryAsync(string id)
        {
            return await _entries.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task CreateTaskEntryAsync(Models.Task task)
        {
            await _entries.InsertOneAsync(task);
        }

        public async System.Threading.Tasks.Task UpdateTaskEntryAsync(string id, Models.Task task)
        {
            await _entries.ReplaceOneAsync(e => e.Id == id, task);
        }

        public async System.Threading.Tasks.Task DeleteTaskEntryAsync(string id)
        {
            await _entries.DeleteOneAsync(e => e.Id == id);
        }
    }
}
