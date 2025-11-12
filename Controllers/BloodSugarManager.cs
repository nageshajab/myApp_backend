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
    public class BloodSugarManager
    {
        private readonly ILogger<BloodSugarManager> _logger;
        private readonly BloodSugarRepository _bloodSugarRepository;

        public BloodSugarManager(ILogger<BloodSugarManager> log, BloodSugarRepository bloodSugarRepository)
        {
            _logger = log;
            _bloodSugarRepository =bloodSugarRepository;
        }

        [FunctionName("createBloodSugar")]
        public async Task<IActionResult> CreateBloodSugar(
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

            BloodSugar newBs = JsonConvert.DeserializeObject<BloodSugar>(requestBody);
            
            newBs.DateTime = newBs.DateTime.ToUniversalTime();

            if (IsValidDate(newBs) == false)
            {
                return new BadRequestObjectResult("Invalid dates data.");
            }

            await _bloodSugarRepository.CreateAsync(newBs);

            return new OkObjectResult(new { message = "Added successfully", data = newBs });
        }

        private bool IsValidDate(BloodSugar dates)
        {
            return dates != null &&
                   dates.DateTime!=DateTime.MinValue &&
                   !string.IsNullOrWhiteSpace(dates.UserId) ;
        }

        [FunctionName("Get")]
        public async Task<IActionResult> Get(
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

            var result = await _bloodSugarRepository.GetAllAsync(userId, searchText, pageNumber);

            return result;
        }

        [FunctionName("updateBloodSugar")]
        public async Task<IActionResult> updateBloodSugar(
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
            BloodSugar date = JsonConvert.DeserializeObject<BloodSugar>(requestBody);

            // Validate the object
            if (!IsValidDate(date))
            {
                return new BadRequestObjectResult("Invalid data ");
            }

            date.DateTime = date.DateTime.ToUniversalTime();//convert to UTC

            var datefromdb = await _bloodSugarRepository.GetAsync(date.Id);

            if (datefromdb == null)
            {
                return new NotFoundObjectResult("date not found.");
            }
            datefromdb.UserId = date.UserId;
            datefromdb.DateTime = date.DateTime;
            datefromdb.Fasting= date.Fasting;
            datefromdb.PP= date.PP;
            datefromdb.Id = date.Id;

            await _bloodSugarRepository.UpdateAsync(date.Id, datefromdb);

            return new OkObjectResult(new { message = "Updated successfully" });
        }


        [FunctionName("bloodSugarDelete")]
        public async Task<IActionResult> bloodSugarDelete(
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
                return new BadRequestObjectResult("Id is required.");
            }

            await _bloodSugarRepository.DeleteAsync(id);

            return new OkObjectResult(new { message = "Deleted successfully" });
        }

        [FunctionName("GetBloodSugar")]      
        public async Task<IActionResult> GetBloodSugar(
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
                return new BadRequestObjectResult("Id is required.");
            }

            var date = await _bloodSugarRepository.GetAsync(id);
            
            return new OkObjectResult(date);
        }
    }
}

