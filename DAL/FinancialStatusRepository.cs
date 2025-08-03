using MongoDB.Driver;
using myazfunction.Models;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace myazfunction.DAL
{
    public class FinancialStatusRepository
    {
        private readonly IMongoCollection<FinancialStatus> _financialStatus;

        public FinancialStatusRepository(MongoDbContext context)
        {
            _financialStatus = context.GetCollection<FinancialStatus>("financialStatus");
        }

        public async Task<FinancialStatus> GetLatestFinancialStatusAsync(string userid)
        {
            var builder = Builders<FinancialStatus>.Filter;
            var filter = builder.Eq(p => p.UserId, userid);
            var sort = Builders<FinancialStatus>.Sort.Descending(p => p.LastUpdatedDate);

            var document = await _financialStatus
                .Find(filter)
                .Sort(sort)
                .Limit(1)
                .FirstOrDefaultAsync();

            return document;
        }

        public async Task<FinancialStatus> GetFinancialStatusAsync(string id)
        {
            var builder = Builders<FinancialStatus>.Filter;
            var filter = builder.Eq(p => p.Id, id);
            var document = await _financialStatus
                .Find(filter)
                .FirstOrDefaultAsync();
            return document;
        }

        public async System.Threading.Tasks.Task CreateFinancialStatusAsync(FinancialStatus financialStatus)
        {
            await _financialStatus.InsertOneAsync(financialStatus);
        }

        public async System.Threading.Tasks.Task UpdateFinancialStatusAsync(string id,FinancialStatus financialStatus)
        {
            await _financialStatus.ReplaceOneAsync(d => d.Id == id, financialStatus);
        }
    }
}
