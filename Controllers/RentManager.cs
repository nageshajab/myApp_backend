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
    public class RentManager
    {
        private readonly ILogger<RentManager> _logger;
        private readonly RentRepository _rentRepository;

        public RentManager(ILogger<RentManager> log, RentRepository rentRepository)
        {
            _logger = log;
            _rentRepository = rentRepository;
        }

        [FunctionName("createRent")]
        public async Task<IActionResult> createRent(
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

            Rent rent = JsonConvert.DeserializeObject<Rent>(requestBody);

            if (IsValidEntry(rent) == false)
            {
                return new BadRequestObjectResult("Invalid rent data.");
            }
            rent.Date = rent.Date.ToUniversalTime();
            await _rentRepository.CreateRentAsync(rent);

            return new OkObjectResult(new { message = "rent added successfully", data = rent });
        }

        private bool IsValidEntry(Rent rent)
        {
            return rent != null &&
                   !string.IsNullOrWhiteSpace(rent.TenantName);
        }

        [FunctionName("GetRents")]
        public async Task<IActionResult> GetRents(
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
           RentSearch rentSearch = JsonConvert.DeserializeObject<RentSearch> (requestBody);

            rentSearch.PageNumber= rentSearch.PageNumber> 0 ? rentSearch.PageNumber : 1;

            if (string.IsNullOrEmpty(rentSearch.UserId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _rentRepository.GetAllRentsAsync(userid: rentSearch.UserId, tenantname: rentSearch.TenantName, pageNumber: rentSearch.PageNumber, month: rentSearch.Month, year: rentSearch.Year);

            return result;
        }

        [FunctionName("GetAllTenants")]
        public async Task<IActionResult> GetAllTenants(
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

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _rentRepository.GetTenantNames(userid: userId);

            return new OkObjectResult(result);
        }

        [FunctionName("GetPendingRents")]
        public async Task<IActionResult> GetPendingRentsAsync(
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
            int month = data?.month;
            int year = data?.year;

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _rentRepository.GetPendingRentsAsync(userid: userId, month: month, year: year);

            return result;
        }

        [FunctionName("updateRent")]
        public async Task<IActionResult> updateRent(
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
            Rent rent = JsonConvert.DeserializeObject<Rent>(requestBody);

            // Validate the object
            if (!IsValidEntry(rent))
            {
                return new BadRequestObjectResult("Invalid rent data");
            }
            rent.Date = rent.Date.ToUniversalTime(); // Convert to universal time   
            var rentfromdb = await _rentRepository.GetRentAsync(rent.Id);

            if (rentfromdb == null)
            {
                return new NotFoundObjectResult("rent not found.");
            }
            rentfromdb.Id = rent.Id;
            rentfromdb.Date = rent.Date;
            rentfromdb.PaidAmount = rent.PaidAmount;
            rentfromdb.RemainingAmount = rent.RemainingAmount;
            rentfromdb.Mseb = rent.Mseb;
            rentfromdb.UserId = rent.UserId;
            rentfromdb.TenantName = rent.TenantName;
            
            await _rentRepository.UpdateRentAsync(rent.Id, rentfromdb);

            return new OkObjectResult(new { message = "rent updated successfully" });
        }


        [FunctionName("DeleteRent")]
        public async Task<IActionResult> DeleteRent(
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

            await _rentRepository.DeleteRentAsync(id);

            return new OkObjectResult(new { message = "Rent deleted successfully" });
        }

        [FunctionName("GetRent")]
        public async Task<IActionResult> GetRent(
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
                return new BadRequestObjectResult("rent Id is required.");
            }

            var rent = await _rentRepository.GetRentAsync(id);

            return new OkObjectResult(rent);
        }
    }

    public class RentSearch
    {   
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; } = 0;
        public int Month { get; set; } = DateTime.Now.Month - 1; // Adjust for zero-based month
        public int Year { get; set; } = DateTime.Now.Year;
        public string UserId { get; set; }
        public string TenantName { get; set; } = string.Empty; // Default to empty string if not provided
    }
}

