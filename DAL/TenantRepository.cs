using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class TenantRepository
    {
        private readonly IMongoCollection<Tenant> _tenantdb; 

        public TenantRepository(MongoDbContext context)
        {
            _tenantdb = context.GetCollection<Tenant>("tenant");
        }

        public async Task<OkObjectResult> GetAllTenantsAsync(string userid, string searchtxt, int pageNumber)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Tenant>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.TenantName, new BsonRegularExpression(searchtxt, "i"));
                filter = builder.And(filter, searchFilter);
            }

            // Get total count for pagination
            var totalCount = await _tenantdb.CountDocumentsAsync(filter);

            // Apply pagination
            var documents = await _tenantdb
                .Find(filter)
                .Skip((pageNumber - 1) * 10)
                .Limit(10)
                .ToListAsync();

            var result = new
            {
                tenants = documents,
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

        public async Task<Tenant> GetTenantAsync(string id)
        {
            return await _tenantdb.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task CreateTenantAsync(Tenant tenant)
        {
            await _tenantdb.InsertOneAsync(tenant);
        }

        public async System.Threading.Tasks.Task UpdateTenantAsync(string id, Tenant tenant)
        {
            await _tenantdb.ReplaceOneAsync(e => e.Id == id, tenant);
        }

        public async System.Threading.Tasks.Task DeleteTenantAsync(string id)
        {
            await _tenantdb.DeleteOneAsync(e => e.Id == id);
        }
    }
}
