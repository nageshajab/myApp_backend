using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class KhataRepository
    {
        private readonly IMongoCollection<Khata> _entries;

        public KhataRepository(MongoDbContext context)
        {
            _entries = context.GetCollection<Khata>("khata");
        }

        public async Task<List<Khata>> GetAllKhataEntriesAsync(string userid)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Khata>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            // Apply pagination
            var documents = await _entries
                .Find(filter)
                .ToListAsync();

            List<Khata> reurnval = new List<Khata>();
            foreach (Khata dt in documents)
            {
                Khata khata = new Khata();
                khata.Id = dt.Id;
                khata.Title = dt.Title;
                khata.Amount = dt.Amount;
                khata.Date = dt.Date;
                khata.UserId = dt.UserId;
                khata.PersonName = dt.PersonName;

                reurnval.Add(khata);
            }
            return reurnval;
        }

        public async Task<OkObjectResult> GetAllKhataEntriesAsync(string userid, string searchtxt, int pageNumber,string personName)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Khata>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.Title, new BsonRegularExpression(searchtxt, "i"));
                filter = builder.And(filter, searchFilter);
            }
            if (!string.IsNullOrEmpty(personName))
            {
                var searchFilter = builder.Regex(p => p.PersonName, new BsonRegularExpression(personName, "i"));
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

            List<Khata> reurnval = new List<Khata>();
            foreach (Khata dt in documents)
            {
                Khata khata= new Khata();
                khata.Id = dt.Id;
                khata.Title = dt.Title;
                khata.Amount= dt.Amount;
                khata.Date = dt.Date;
                khata.UserId=dt.UserId;
                khata.PersonName = dt.PersonName;

                reurnval.Add(khata);
            }
            var distinctPersonNames = await GetDistinctPersonNames(userid);
            var result = new
            {
                khataEntries = reurnval,
                distinctPersonNames= distinctPersonNames,
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

        public async Task<Khata> GetKhataEntryAsync(string id)
        {
            return await _entries.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetDistinctPersonNames(string userid)
        {
            var filter = Builders<Khata>.Filter.Eq(x => x.UserId, userid);
            var distinctPersonNames = await _entries.Distinct(x => x.PersonName, filter).ToListAsync();
            return distinctPersonNames;
        }

        public async System.Threading.Tasks.Task CreateKhataEntryAsync(Khata khata)
        {
            await _entries.InsertOneAsync(khata);
        }

        public async System.Threading.Tasks.Task UpdateKhataEntryAsync(string id, Khata khata)
        {
            await _entries.ReplaceOneAsync(e => e.Id == id, khata);
        }

        public async System.Threading.Tasks.Task DeleteKhataEntryAsync(string id)
        {
            await _entries.DeleteOneAsync(e => e.Id == id);
        }
    }
}
