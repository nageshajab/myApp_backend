using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using myazfunction.DAL;
using myazfunction.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace myazfunction.Controllers
{
    public class FinancialManager
    {
        private readonly ILogger<FinancialManager> _logger;
        private readonly FinancialStatusRepository _financialStatusRepository;

        public FinancialManager(ILogger<FinancialManager> log, FinancialStatusRepository financialStatusRepository)
        {
            _logger = log;
            _financialStatusRepository = financialStatusRepository;
        }

        private bool IsValid(FinancialStatus financialStatus)
        {
            return financialStatus != null &&
                   financialStatus.LastUpdatedDate != DateTime.MinValue &&
                   !string.IsNullOrWhiteSpace(financialStatus.UserId);
        }

        [FunctionName("updateFinancialStatus")]
        public async Task<IActionResult> updateFinancialStatus(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            // Set CORS headers on the response
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            // Handle preflight OPTIONS request
            if (req.Method == HttpMethods.Options)
            {
                return new OkResult(); // No body needed for preflight
            }

            // Read and deserialize the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            FinancialStatus financialstatus = JsonConvert.DeserializeObject<FinancialStatus>(requestBody);

            // Validate the object
            if (!IsValid(financialstatus))
            {
                return new BadRequestObjectResult("Invalid data ");
            }

            financialstatus.LastUpdatedDate = financialstatus.LastUpdatedDate.ToUniversalTime();//convert to UTC

            var financialstatusfromdb = await _financialStatusRepository.GetFinancialStatusAsync(financialstatus.Id);

            if (financialstatusfromdb == null)
            {
                await _financialStatusRepository.CreateFinancialStatusAsync(financialstatus);
                return new OkObjectResult(new { message = " created successfully" });
            }
            financialstatusfromdb.UserId = financialstatus.UserId;
            financialstatusfromdb.mutualFundsCurrentValue = financialstatus.mutualFundsCurrentValue;
            financialstatusfromdb.mutualFundsInvestAmount = financialstatus.mutualFundsInvestAmount;

            financialstatusfromdb.stocksCurrentValue = financialstatus.stocksCurrentValue;
            financialstatusfromdb.stocksInvestamount = financialstatus.stocksInvestamount;

            financialstatusfromdb.pensionContribution = financialstatus.pensionContribution;

            financialstatusfromdb.HdfcBankFdAmount = financialstatus.HdfcBankFdAmount;
            financialstatusfromdb.HdfcBankSavingsAccountBalance = financialstatus.HdfcBankSavingsAccountBalance;

            financialstatusfromdb.IciciBankSavingsAccountBalance = financialstatus.IciciBankSavingsAccountBalance;
            financialstatusfromdb.ICICIFdAmount = financialstatus.ICICIFdAmount;

            financialstatusfromdb.KotakBankFdAmount = financialstatus.KotakBankFdAmount;
            financialstatusfromdb.KotakBankSavingsAccountBalance = financialstatus.KotakBankSavingsAccountBalance;
            financialstatusfromdb.SriramFD=financialstatus.SriramFD;

            financialstatusfromdb.Id = financialstatus.Id;

            financialstatusfromdb.LastUpdatedDate = financialstatus.LastUpdatedDate;

            financialstatusfromdb.PfEmployeeShare = financialstatus.PfEmployeeShare;
            financialstatusfromdb.PfEmployerShare = financialstatus.PfEmployerShare;
            financialstatusfromdb.pensionContribution = financialstatus.pensionContribution;

            await _financialStatusRepository.UpdateFinancialStatusAsync(financialstatus.Id, financialstatusfromdb);

            return new OkObjectResult(new { message = " updated successfully" });
        }

        [FunctionName("GetFinancialStatus")]
        public async Task<IActionResult> GetFinancialStatus(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {

            _logger.LogInformation("C# HTTP trigger function processed a request to get a password by Id.");
            // Set CORS headers on the response
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            // Handle preflight OPTIONS request
            if (req.Method == HttpMethods.Options)
            {
                return new OkResult(); // No body needed for preflight
            }
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string userId = data?.userid;
            string id = data?.id;

            FinancialStatus financialStatus;

            try
            {
                if (string.IsNullOrEmpty(id))
                    financialStatus = await _financialStatusRepository.GetLatestFinancialStatusAsync(userId);
                else
                    financialStatus = await _financialStatusRepository.GetFinancialStatusAsync(id);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Error retrieving financial status: {ex.Message}");
            }
            return new OkObjectResult(financialStatus);
        }
    }
}

