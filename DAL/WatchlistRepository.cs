using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class WatchlistRepository
    {
        private readonly IMongoCollection<Watchlist> _entries;

        public WatchlistRepository(MongoDbContext context)
        {
            _entries = context.GetCollection<Watchlist>("WatchlistRepository");
        }

        public async Task<OkObjectResult> GetAllWatchlistEntriesAsync(string userid, string searchtxt, int pageNumber)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Watchlist>.Filter;
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

            List<Watchlist> reurnval = new List<Watchlist>();
            foreach (Watchlist dt in documents)
            {
                Watchlist khata = new Watchlist();
                khata.Id = dt.Id;
                khata.Title = dt.Title;
                khata.Date = dt.Date;
                khata.Status = dt.Status;
                khata.UserId=dt.UserId;
                khata.Type = dt.Type;
                khata.Language = dt.Language;
                khata.Genre = dt.Genre;
                khata.Rating = dt.Rating;
                khata.Ott = dt.Ott;

                reurnval.Add(khata);
            }
        
            var result = new
            {
                items = reurnval,
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

        public async Task<Watchlist> GetWatchlistItemAsync(string id)
        {
            return await _entries.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task CreateWatchlistEntryAsync(Watchlist tra)
        {
            await _entries.InsertOneAsync(tra);
        }

        public async System.Threading.Tasks.Task UpdateWatchlistItemAsync(string id, Watchlist tra)
        {
            await _entries.ReplaceOneAsync(e => e.Id == id, tra);
        }

        public async System.Threading.Tasks.Task DeleteWatchlistItemAsync(string id)
        {
            await _entries.DeleteOneAsync(e => e.Id == id);
        }
    }
}
