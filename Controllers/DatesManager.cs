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
    public class DatesManager
    {
        private readonly ILogger<DatesManager> _logger;
        private readonly DatesRepository _datesRepository;

        public DatesManager(ILogger<DatesManager> log, DatesRepository datesRepository)
        {
            _logger = log;
            _datesRepository = datesRepository;
        }

        [FunctionName("createdate")]
        public async Task<IActionResult> CreateDate(
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

            Dates newDates = JsonConvert.DeserializeObject<Dates>(requestBody);

            if (IsValidDate(newDates) == false)
            {
                return new BadRequestObjectResult("Invalid dates data.");
            }

            await _datesRepository.CreateDateAsync(newDates);

            return new OkObjectResult(new { message = "Dates added successfully", data = newDates });
        }

        private bool IsValidDate(Dates dates)
        {
            return dates != null &&
                   !string.IsNullOrWhiteSpace(dates.Date) &&
                   !string.IsNullOrWhiteSpace(dates.userid) &&
                   !string.IsNullOrWhiteSpace(dates.Title);
        }

        [FunctionName("GetDates")]
        public async Task<IActionResult> GetDates(
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

            var result = await _datesRepository.GetAllDatesAsync(userId, searchText, pageNumber);

            return result;
        }

        [FunctionName("updateDate")]
        public async Task<IActionResult> UpdateDate(
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
            Dates date = JsonConvert.DeserializeObject<Dates>(requestBody);

            // Validate the object
            if (!IsValidDate(date))
            {
                return new BadRequestObjectResult("Invalid date data ");
            }

            var datefromdb = await _datesRepository.GetDateAsync(date.Id);
            if (datefromdb == null)
            {
                return new NotFoundObjectResult("date not found.");
            }
            datefromdb.userid = date.userid;
            datefromdb.Title = date.Title;
            datefromdb.Date = date.Date;
            datefromdb.Description = date.Description;
            datefromdb.Duration = date.Duration;
            datefromdb.isRecurring = date.isRecurring;
            datefromdb.RecurringEvent = date.RecurringEvent;
            datefromdb.Id = date.Id;

            await _datesRepository.UpdateDateAsync(date.Id, datefromdb);

            return new OkObjectResult(new { message = "Date updated successfully" });
        }


        [FunctionName("DateDelete")]
        public async Task<IActionResult> DateDelete(
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
                return new BadRequestObjectResult("Password Id is required.");
            }

            await _datesRepository.DeleteDateAsync(id);

            return new OkObjectResult(new { message = "Date deleted successfully" });
        }

        [FunctionName("DateGet")]      
        public async Task<IActionResult> DateGet(
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
                return new BadRequestObjectResult("Date Id is required.");
            }

            var date = await _datesRepository.GetDateAsync(id);
            date.Duration=Dates.CalculateDuration(DateTime.Parse(date.Date));

            return new OkObjectResult(date);
        }

    }
}

