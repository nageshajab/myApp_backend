using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class BloodSugarRepository
    {
        private readonly IMongoCollection<BloodSugar> _bloodSugars;

        public BloodSugarRepository(MongoDbContext context)
        {
            _bloodSugars= context.GetCollection<BloodSugar>("bloodSugars");
        }

        public async Task<List<BloodSugar>> GetAllAsync(string userid)
        {
            // Build filter
            var builder = Builders<BloodSugar>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            // Apply pagination
            var documents = await _bloodSugars
                .Find(filter)
                .ToListAsync();

            List<BloodSugar> reurnval = new();

            foreach (BloodSugar dt in documents)
            {
                BloodSugar bs = new BloodSugar();
                bs.Id = dt.Id;
                bs.DateTime = dt.DateTime;
                bs.Fasting = dt.Fasting;
                bs.PP= dt.PP;
                bs.UserId=dt.UserId;
                reurnval.Add(bs);
            }
            
            return reurnval;
        }

        public async Task<OkObjectResult> GetAllAsync(string userid,string searchtxt,int pageNumber)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<BloodSugar>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.DateTime, new BsonRegularExpression(searchtxt, "i"));
                filter = builder.And(filter, searchFilter);
            }

            // Get total count for pagination
            var totalCount = await _bloodSugars.CountDocumentsAsync(filter);

            // Apply pagination
            var documents = await _bloodSugars
                .Find(filter)
                .Skip((pageNumber - 1) * 10)
                .Limit(10)
                .ToListAsync();

            List<BloodSugar> reurnval= new ();

            foreach(BloodSugar dt in documents)
            {
                BloodSugar bs = new BloodSugar();
                bs.Id = dt.Id;
                bs.DateTime = dt.DateTime;
                bs.Fasting = dt.Fasting;
                bs.PP = dt.PP;
                bs.UserId = dt.UserId;
                reurnval.Add(bs);
            }

            var result = new
            {
                bloodSugars= reurnval,
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

        public async Task<BloodSugar> GetAsync(string id)
        {
            return await _bloodSugars.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task CreateAsync(BloodSugar bloodSugar)
        {            
            await _bloodSugars.InsertOneAsync(bloodSugar);           
        }

        public async System.Threading.Tasks.Task UpdateAsync(string id, BloodSugar bloodSugar)
        {
            await _bloodSugars.ReplaceOneAsync(d=> d.Id == id, bloodSugar);
        }

        public async System.Threading.Tasks.Task DeleteAsync(string id)
        {
            await _bloodSugars.DeleteOneAsync(d => d.Id == id);
        }
    }
}
