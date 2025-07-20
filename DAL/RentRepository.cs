using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class RentRepository
    {
        private readonly IMongoCollection<Rent> _entries;
        private readonly IMongoCollection<Tenant> _tenantdb; 

        public RentRepository(MongoDbContext context)
        {
            _entries = context.GetCollection<Rent>("rent");
            _tenantdb = context.GetCollection<Tenant>("tenant");
        }

        public async Task<OkObjectResult> GetAllRentsAsync(string userid, string tenantname, int pageNumber, int month, int year)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Rent>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            if (!string.IsNullOrEmpty(tenantname))
            {
                var searchFilter = builder.Eq(p => p.TenantName, tenantname);
                filter = builder.And(filter, searchFilter);
            }
            //month += 1; // Adjust month to be 1-based (0 means January, 11 means December)
            if (month > 0 && year > 0)
            {
                var dateFilter = builder.And(
                    builder.Gte(p => p.Date, new DateTime(year, month, 1)),
                    builder.Lt(p => p.Date  , new DateTime(year, month, 1).AddMonths(1))
                );
                filter = builder.And(filter, dateFilter);
            }

            // Get total count for pagination
            var totalCount = await _entries.CountDocumentsAsync(filter);

            // Apply pagination
            var documents = await _entries
                .Find(filter)
                .Skip((pageNumber - 1) * 10)
                .Limit(10)
                .ToListAsync();

           
            var tenants = await GetTenantNames(userid);
            var result = new
            {
                rents = documents,
                tenants= tenants,
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

        public async Task<Rent> GetRentAsync(string id)
        {
            return await _entries.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetTenantNames(string userid)
        {
            var builder = Builders<Tenant>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            var tenants = await _tenantdb.Distinct(x => x.TenantName, filter).ToListAsync();

            return tenants;
        }

        public async System.Threading.Tasks.Task CreateRentAsync(Rent rent)
        {
            await _entries.InsertOneAsync(rent);
        }

        public async System.Threading.Tasks.Task UpdateRentAsync(string id, Rent rent)
        {
            await _entries.ReplaceOneAsync(e => e.Id == id, rent);
        }

        public async System.Threading.Tasks.Task DeleteRentAsync(string id)
        {
            await _entries.DeleteOneAsync(e => e.Id == id);
        }
    }
}
