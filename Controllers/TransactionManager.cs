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
    public class TransactionManager
    {
        private readonly ILogger<TransactionManager> _logger;
        private readonly TransactionRepository _transactionRepository ;

        public TransactionManager(ILogger<TransactionManager> log, TransactionRepository transactionRepository)
        {
            _logger = log;
            _transactionRepository = transactionRepository;
        }

        [FunctionName("createTransactionEntry")]
        public async Task<IActionResult> CreateTransactionEntry(
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

            string UserId = req.Query["UserId"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

           Transactions transactions = JsonConvert.DeserializeObject<Transactions>(requestBody);

            if (IsValidEntry(transactions) == false)
            {
                return new BadRequestObjectResult("Invalid khata entry data.");
            }
            transactions.Date = transactions.Date.ToUniversalTime();
            await _transactionRepository.CreateTransactionEntryAsync(transactions);

            return new OkObjectResult(new { message = "Transaction Entry added successfully", data = transactions });
        }

        private bool IsValidEntry(Transactions tra)
        {
            return tra!= null &&
                   !string.IsNullOrWhiteSpace(tra.Title) &&
                   !tra.Date.Equals(DateTime.MinValue) &&
                   !string.IsNullOrWhiteSpace(tra.Amount);
        }

        [FunctionName("GetTransactionEntries")]
        public async Task<IActionResult> GetTransactionEntries(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
       ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

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
            string searchText = data?.searchtxt;
            int pageNumber = data?.pageNumber;
            pageNumber = pageNumber > 0 ? pageNumber : 1;

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _transactionRepository.GetAllTransactionsEntriesAsync(userId, searchText, pageNumber);

            return result;
        }

        [FunctionName("updateTransactionEntry")]
        public async Task<IActionResult> updateTransactionEntry(
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
            Transactions transactions= JsonConvert.DeserializeObject<Transactions>(requestBody);

            // Validate the object
            if (!IsValidEntry(transactions))
            {
                return new BadRequestObjectResult("Invalid transaction data ");
            }
            transactions.Date = transactions.Date.ToUniversalTime(); // Convert to universal time
            var transactionfromdb = await _transactionRepository.GetTransactionEntryAsync(transactions.Id);

            if (transactionfromdb == null)
            {
                return new NotFoundObjectResult("transaction not found.");
            }
            transactionfromdb.UserId = transactions.UserId;
            transactionfromdb.Title = transactions.Title;
            transactionfromdb.Date = transactions.Date;
            transactionfromdb.Amount= transactions.Amount;
            transactionfromdb.UserId= transactions.UserId;
            transactionfromdb.Description = transactions.Description;
            transactionfromdb.Id = transactions.Id;

            await _transactionRepository.UpdateTransactionEntryAsync(transactions.Id, transactionfromdb);

            return new OkObjectResult(new { message = "Transaction Entry updated successfully" });
        }


        [FunctionName("Deletetransaction")]
        public async Task<IActionResult> transactionDelete(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to delete a password.");
            // Set CORS headers on the response
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            // Handle preflight OPTIONS request
            if (req.Method == HttpMethods.Options)
            {
                return new OkResult(); // No body needed for preflight
            }

            string id = req.Query["id"];

            if (string.IsNullOrEmpty(id))
            {
                return new BadRequestObjectResult(" Id is required.");
            }

            await _transactionRepository.DeleteTransactionEntryAsync(id);

            return new OkObjectResult(new { message = "Transaction Entry deleted successfully" });
        }

        [FunctionName("getTransaction")]
        public async Task<IActionResult> transactionGet(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
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

            string id = req.Query["id"];

            if (string.IsNullOrEmpty(id))
            {
                return new BadRequestObjectResult("transaction Id is required.");
            }

            var tra = await _transactionRepository.GetTransactionEntryAsync(id);
            
            return new OkObjectResult(tra);
        }
    }
}

