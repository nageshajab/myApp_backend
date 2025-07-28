using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class PasswordRepository
    {
        private readonly IMongoCollection<Passwords> _passwords;

        public PasswordRepository(MongoDbContext context)
        {
            _passwords= context.GetCollection<Passwords>("mypasswords");
        }

        public async Task<List<Passwords>> GetAllPasswordsAsync(string userid)
        {
            // Build filter
            var builder = Builders<Passwords>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            // Apply pagination
            var documents = await _passwords
                .Find(filter)
                .ToListAsync();

            return documents;
        }

        public async Task<OkObjectResult> GetAllPasswordsAsync(string userid, string searchtxt, int pageNumber)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Passwords>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.System, new BsonRegularExpression(searchtxt, "i"));
                filter = builder.And(filter, searchFilter);
            }

            // Get total count for pagination
            var totalCount = await _passwords.CountDocumentsAsync(filter);

            // Apply pagination
            var documents = await _passwords
                .Find(filter)
                .Skip((pageNumber - 1) * 10)
                .Limit(10)
                .ToListAsync();

           
            var result = new
            {
                dates = documents,
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

        public async Task<Passwords> GetPasswordAsync(string id)
        {
            return await _passwords.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task CreatePasswordAsync(Passwords passwords)
        {
            await _passwords.InsertOneAsync(passwords);
        }

        public async System.Threading.Tasks.Task UpdatePasswordAsync(string id, Passwords passwords)
        {
            await _passwords.ReplaceOneAsync(d => d.Id == id, passwords);
        }

        public async System.Threading.Tasks.Task DeletePasswordAsync(string id)
        {
            await _passwords.DeleteOneAsync(d => d.Id == id);
        }
    }
}
