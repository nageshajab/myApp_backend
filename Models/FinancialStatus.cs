using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace myazfunction.Models
{
    public class FinancialStatus
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public int IciciBankSavingsAccountBalance { get; set; }
        public int ICICIFdAmount { get; set; }
        public int HdfcBankSavingsAccountBalance { get; set; }
        public int HdfcBankFdAmount { get; set; }
        public int KotakBankSavingsAccountBalance { get; set; }
        public int KotakBankFdAmount { get; set; }

        public int stocksInvestamount { get; set; }
        public int stocksCurrentValue { get; set; }

        public int mutualFundsInvestAmount { get; set; }
        public int mutualFundsCurrentValue { get; set; }

        public int PfEmployeeShare { get; set; }
        public int PfEmployerShare { get; set; }
        public int pensionContribution { get; set; }
    } 
}
