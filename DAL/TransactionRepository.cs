using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class TransactionRepository
    {
        private readonly IMongoCollection<Transactions> _entries;

        public TransactionRepository(MongoDbContext context)
        {
            _entries = context.GetCollection<Transactions>("transactions");
        }

        public async Task<List<Transactions>> GetAllTransactionsEntriesAsync(string userid)
        {
            // Build filter
            var builder = Builders<Transactions>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);
            // Apply pagination
            var documents = await _entries
                .Find(filter)
                .ToListAsync();
            return documents;
        }
     
        public async Task<OkObjectResult> GetAllTransactionsEntriesAsync(string userid, string searchtxt, int pageNumber)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Transactions>.Filter;
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

            List<Transactions> reurnval = new List<Transactions>();
            foreach (Transactions dt in documents)
            {
                Transactions khata = new Transactions();
                khata.Id = dt.Id;
                khata.Title = dt.Title;
                khata.Amount = dt.Amount;
                khata.Date = dt.Date;
                khata.UserId = dt.UserId;
                khata.Description = dt.Description;

                reurnval.Add(khata);
            }

            var result = new
            {
                TransactionEntries = reurnval,
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
     
        public async Task<Transactions> GetTransactionEntryAsync(string id)
        {
            return await _entries.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task CreateTransactionEntryAsync(Transactions tra)
        {
            await _entries.InsertOneAsync(tra);
        }

        public async System.Threading.Tasks.Task UpdateTransactionEntryAsync(string id, Transactions tra)
        {
            await _entries.ReplaceOneAsync(e => e.Id == id, tra);
        }

        public async System.Threading.Tasks.Task DeleteTransactionEntryAsync(string id)
        {
            await _entries.DeleteOneAsync(e => e.Id == id);
        }
    }
}
