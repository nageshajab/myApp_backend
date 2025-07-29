using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myazfunction.DAL
{
    public class DocumentRepository
    {
        private readonly IMongoCollection<Document> _documents;

        public DocumentRepository(MongoDbContext context)
        {
            _documents= context.GetCollection<Document>("documents");
        }

        public async Task<OkObjectResult> GetAllDocumentsAsync(string userid, string searchtxt, int pageNumber)
        {
            int pageSize = 10;
            // Build filter
            var builder = Builders<Document>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);

            if (!string.IsNullOrEmpty(searchtxt))
            {
                var searchFilter = builder.Regex(p => p.Title, new BsonRegularExpression(searchtxt, "i"));
                filter = builder.And(filter, searchFilter);
            }

            // Get total count for pagination
            var totalCount = await _documents.CountDocumentsAsync(filter);

            // Apply pagination
            var documents = await _documents
                .Find(filter)
                .Skip((pageNumber - 1) * 10)
                .Limit(10)
                .ToListAsync();
           
            var result = new
            {
                documents = documents,
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

        public async Task<Document> GetDocumentAsync(string id)
        {
            return await _documents.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task CreateDocumentAsync(Document document)
        {
            await _documents.InsertOneAsync(document);
        }

        public async System.Threading.Tasks.Task UpdateDocumentAsync(string id, Document document)
        {
            await _documents.ReplaceOneAsync(d => d.Id == id, document);
        }

        public async System.Threading.Tasks.Task DeleteDocumentAsync(string id)
        {
            await _documents.DeleteOneAsync(d => d.Id == id);
        }
    }
}
