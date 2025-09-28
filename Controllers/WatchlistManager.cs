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
    public class WatchlistManager
    {
        private readonly ILogger<WatchlistManager> _logger;
        private readonly WatchlistRepository _watchlistRepository ;

        public WatchlistManager(ILogger<WatchlistManager> log, WatchlistRepository watchlistRepository)
        {
            _logger = log;
            _watchlistRepository = watchlistRepository;
        }

        [FunctionName("createWatchlistItem")]
        public async Task<IActionResult> createWatchlistItem(
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

           Watchlist watchlist = JsonConvert.DeserializeObject<Watchlist>(requestBody);

            if (IsValid(watchlist) == false)
            {
                return new BadRequestObjectResult("Invalid khata entry data.");
            }
            watchlist.Date = watchlist.Date.ToUniversalTime();
            await _watchlistRepository.CreateWatchlistEntryAsync(watchlist);

            return new OkObjectResult(new { message = "Watchlist item added successfully", data = watchlist });
        }

        private bool IsValid(Watchlist tra)
        {
            return tra != null &&
                   !string.IsNullOrWhiteSpace(tra.Title) &&
                   !tra.Date.Equals(DateTime.MinValue) 
                  ;
        }

        [FunctionName("GetWatchlistItems")]
        public async Task<IActionResult> GetWatchlistItems(
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
            string userId = data?.userId;
            string searchText = data?.searchText;
            int pageNumber = data?.pageNumber;
            pageNumber = pageNumber > 0 ? pageNumber : 1;

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _watchlistRepository.GetAllWatchlistEntriesAsync(userId, searchText, pageNumber);

            return result;
        }

        [FunctionName("updateWatchlistItem")]
        public async Task<IActionResult> updateWatchlistItem(
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
            Watchlist watchlist= JsonConvert.DeserializeObject<Watchlist>(requestBody);

            // Validate the object
            if (!IsValid(watchlist))
            {
                return new BadRequestObjectResult("Invalid watchlist item");
            }
            watchlist.Date = watchlist.Date.ToUniversalTime(); // Convert to universal time
            var watchlistitemfromdb = await _watchlistRepository.GetWatchlistItemAsync(watchlist.Id);

            if (watchlistitemfromdb== null)
            {
                return new NotFoundObjectResult("watchlist item not found.");
            }
            watchlistitemfromdb.Id = watchlist.Id;
            watchlistitemfromdb.Title = watchlist.Title;
            watchlistitemfromdb.Date = watchlist.Date;
            watchlistitemfromdb.Status = watchlist.Status;
            watchlistitemfromdb.UserId = watchlist.UserId;
            watchlistitemfromdb.Type= watchlist.Type;
            watchlistitemfromdb.Language= watchlist.Language;
            watchlistitemfromdb.Genre = watchlist.Genre;
            watchlistitemfromdb.Rating = watchlist.Rating;
            watchlistitemfromdb.Ott = watchlist.Ott;

            await _watchlistRepository.UpdateWatchlistItemAsync(watchlist.Id, watchlistitemfromdb);

            return new OkObjectResult(new { message = "watchlist item updated successfully" });
        }

        [FunctionName("DeleteWatchlistitem")]
        public async Task<IActionResult> DeleteWatchlistitem(
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

            await _watchlistRepository.DeleteWatchlistItemAsync(id);

            return new OkObjectResult(new { message = "Watchlist item deleted successfully" });
        }

        [FunctionName("GetWatchlistitem")]
        public async Task<IActionResult> GetWatchlistitem(
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
                return new BadRequestObjectResult("GetWatchlistitem Id is required.");
            }

            var tra = await _watchlistRepository.GetWatchlistItemAsync(id);
            
            return new OkObjectResult(tra);
        }
    }
}

