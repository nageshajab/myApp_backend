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

           Rent rent= JsonConvert.DeserializeObject<Rent>(requestBody);

            if (IsValidEntry(rent) == false)
            {
                return new BadRequestObjectResult("Invalid rent data.");
            }

            await _rentRepository.CreateRentAsync(rent);

            return new OkObjectResult(new { message = "rent added successfully", data = rent});
        }

        private bool IsValidEntry(Rent rent)
        {
            return rent!= null &&
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
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string userId = data?.userid;
            string tenant= data?.tenant;
            int pageNumber = data?.pageNumber;
            int month = data?.month;
            int year = data?.year;

            pageNumber = pageNumber > 0 ? pageNumber : 1;

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _rentRepository.GetAllRentsAsync(userid: userId,tenantname: tenant,pageNumber: pageNumber,month:month,year:year);

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

            return new OkObjectResult( result);
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
            Rent rent= JsonConvert.DeserializeObject<Rent>(requestBody);

            // Validate the object
            if (!IsValidEntry(rent))
            {
                return new BadRequestObjectResult("Invalid rent data");
            }

            var rentfromdb= await _rentRepository.GetRentAsync(rent.Id);

            if (rentfromdb== null)
            {
                return new NotFoundObjectResult("rent not found.");
            }
            rentfromdb.Id = rent.Id;
            rentfromdb.Date= rent.Date;
            rentfromdb.PaidAmount= rent.PaidAmount;
            rentfromdb.RemainingAmount=rent.RemainingAmount;
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

            var rent= await _rentRepository.GetRentAsync(id);
            
            return new OkObjectResult(rent);
        }
    }
}

