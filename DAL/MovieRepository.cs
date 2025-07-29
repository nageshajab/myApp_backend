using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class MovieRepository
    {
        private readonly IMongoCollection<Movie> _entries;

        public MovieRepository(MongoDbContext context)
        {
            _entries = context.GetCollection<Movie>("movies");
        }

        public async Task<OkObjectResult> GetAllMoviesAsync(string userid, string searchtxt, int pageNumber)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Movie>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.tags, new BsonRegularExpression(searchtxt, "i"));
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
                    
            var result = new
            {
                movies= documents,
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

        public async Task<Movie> GetMovieAsync(string id)
        {
            return await _entries.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task CreateMovieAsync(Movie tra)
        {
            await _entries.InsertOneAsync(tra);
        }

        public async System.Threading.Tasks.Task UpdateMovieAsync(string id, Movie tra)
        {
            await _entries.ReplaceOneAsync(e => e.Id == id, tra);
        }

        public async System.Threading.Tasks.Task DeleteMovieAsync(string id)
        {
            await _entries.DeleteOneAsync(e => e.Id == id);
        }

        public async Task<Byte[]> GetImage(string id)
        {
            var item= await _entries.Find(e => e.Id == id).FirstOrDefaultAsync();
            return item?.ImageData;
        }
    }
}
