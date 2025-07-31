using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class MovieRepository
    {
        int pageSize = 12;
        private readonly IMongoCollection<Movie> _entries;
        private readonly IMongoCollection<MovieTags> _tags;

        public MovieRepository(MongoDbContext context)
        {
            _entries = context.GetCollection<Movie>("movies");
            _tags = context.GetCollection<MovieTags>("MovieTags");
        }

        public async Task<OkObjectResult> GetAllMoviesAsync(string userid, string searchtxt, string selectedTags, int pageNumber,bool isJav)
        {            
            // Build filter
            var builder = Builders<Movie>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.Title, new BsonRegularExpression(searchtxt, "i"));
                filter = builder.And(filter, searchFilter);
            }

            if (!string.IsNullOrEmpty(selectedTags))
            {
                var tagsArray = selectedTags.Split(',').Select(t => t.Trim()).ToList();
                var searchFilter = builder.Or(tagsArray.Select(tag => builder.Regex("tags", new BsonRegularExpression(tag, "i"))).ToArray());
                filter = builder.And(filter, searchFilter);
            }

            // Get total count for pagination
            var totalCount = await _entries.CountDocumentsAsync(filter);

            // Apply pagination
            var documents = await _entries
                .Find(filter)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var alltags = await GetAllTagsAsync(userid);
            var result = new
            {
                movies = documents,
                tags = alltags,
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

        public async Task<List<string>> GetAllTagsAsync(string userid)
        {
            var tags = await _tags
                .Find(e => e.UserId == userid)
                .Project(e => e.Tag)
                .ToListAsync();

            return tags;
        }

        public async System.Threading.Tasks.Task InsertMissingTags(Movie tra)
        {
            var temp = tra.tags.Split(",", StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in temp)
            {
                var foundtag = await _tags.Find(e => e.Tag == t.Trim() && e.UserId == tra.UserId).FirstOrDefaultAsync();

                if (foundtag == null)
                {
                    var newTag = new MovieTags
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        UserId = tra.UserId,
                        Tag = t.Trim()
                    };
                    await _tags.InsertOneAsync(newTag);
                }
            }
        }

        public async System.Threading.Tasks.Task CreateMovieAsync(Movie tra)
        {
            await InsertMissingTags(tra);
            await _entries.InsertOneAsync(tra);
        }

        public async System.Threading.Tasks.Task UpdateMovieAsync(string id, Movie tra)
        {
            await InsertMissingTags(tra);
            await _entries.ReplaceOneAsync(e => e.Id == id, tra);
        }

        public async System.Threading.Tasks.Task DeleteMovieAsync(string id)
        {
            await _entries.DeleteOneAsync(e => e.Id == id);
        }

        public async Task<Byte[]> GetImage(string id)
        {
            var item = await _entries.Find(e => e.Id == id).FirstOrDefaultAsync();
            return item?.ImageData;
        }
    }
}
