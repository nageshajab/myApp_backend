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
    public class KhataManager
    {
        private readonly ILogger<KhataManager> _logger;
        private readonly KhataRepository _khataRepository;

        public KhataManager(ILogger<KhataManager> log, KhataRepository khataRepository)
        {
            _logger = log;
            _khataRepository = khataRepository;
        }

        [FunctionName("createkhataentry")]
        public async Task<IActionResult> CreateKhataEntry(
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

           Khata khata  = JsonConvert.DeserializeObject<Khata>(requestBody);

            if (IsValidEntry(khata) == false)
            {
                return new BadRequestObjectResult("Invalid khata entry data.");
            }
            khata.Date=khata.Date.ToUniversalTime();//to universal time
            await _khataRepository.CreateKhataEntryAsync(khata);

            return new OkObjectResult(new { message = "Khata Entry added successfully", data = khata });
        }

        private bool IsValidEntry(Khata khata)
        {
            return khata!= null &&
                   !string.IsNullOrWhiteSpace(khata.Title) &&
                   !khata.Date.Equals(DateTime.MinValue) &&
                   !string.IsNullOrWhiteSpace(khata.Amount);
        }

        [FunctionName("GetKhataEntries")]
        public async Task<IActionResult> GetKhataEntries(
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
            string personName = data?.personName;
            pageNumber = pageNumber > 0 ? pageNumber : 1;

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _khataRepository.GetAllKhataEntriesAsync(userId, searchText, pageNumber, personName);

            return result;
        }

        [FunctionName("updateKhataEntry")]
        public async Task<IActionResult> updateKhataEntry(
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
            Khata khata= JsonConvert.DeserializeObject<Khata>(requestBody);
            khata.Date = khata.Date.ToUniversalTime(); // Convert to universal time
            // Validate the object
            if (!IsValidEntry(khata))
            {
                return new BadRequestObjectResult("Invalid khata data ");
            }

            var khatafromdb = await _khataRepository.GetKhataEntryAsync(khata.Id);
            if (khatafromdb == null)
            {
                return new NotFoundObjectResult("khata not found.");
            }
            khatafromdb.UserId = khata.UserId;
            khatafromdb.Title = khata.Title;
            khatafromdb.Date = khata.Date;
            khatafromdb.Amount= khata.Amount;
            khatafromdb.UserId= khata.UserId;
            khatafromdb.PersonName = khata.PersonName;
            khatafromdb.Id = khata.Id;

            await _khataRepository.UpdateKhataEntryAsync(khata.Id, khatafromdb);

            return new OkObjectResult(new { message = "Khata Entry updated successfully" });
        }


        [FunctionName("KhataDelete")]
        public async Task<IActionResult> KhataDelete(
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

            await _khataRepository.DeleteKhataEntryAsync(id);

            return new OkObjectResult(new { message = "Khata Entry deleted successfully" });
        }

        [FunctionName("KhataGet")]
        public async Task<IActionResult> KhataGet(
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
                return new BadRequestObjectResult("khata Id is required.");
            }

            var khata = await _khataRepository.GetKhataEntryAsync(id);
            
            return new OkObjectResult(khata);
        }
    }
}

