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
    public class EventsManager
    {
        private readonly ILogger<EventsManager> _logger;
        private readonly EventsRepository _EventsRepository;

        public EventsManager(ILogger<EventsManager> log, EventsRepository EventsRepository)
        {
            _logger = log;
            _EventsRepository = EventsRepository;
        }

        [FunctionName("createEvent")]
        public async Task<IActionResult> CreateEvent(
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

            Events newEvents = JsonConvert.DeserializeObject<Events>(requestBody);
            newEvents.Date = newEvents.Date.ToUniversalTime();
            if (IsValidEvent(newEvents) == false)
            {
                return new BadRequestObjectResult("Invalid Events data.");
            }

            await _EventsRepository.CreateEventAsync(newEvents);

            return new OkObjectResult(new { message = "Events added successfully", data = newEvents });
        }

        private bool IsValidEvent(Events Events)
        {
            return Events != null &&
                   Events.Date!=DateTime.MinValue &&
                   !string.IsNullOrWhiteSpace(Events.userid) &&
                   !string.IsNullOrWhiteSpace(Events.Title);
        }

        [FunctionName("GetEvents")]
        public async Task<IActionResult> GetEvents(
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
            Boolean showall= data?.showAll;
            pageNumber = pageNumber > 0 ? pageNumber : 1;

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _EventsRepository.GetAllEventsAsync(userId, searchText, pageNumber,showall);

            return result;
        }

        [FunctionName("updateEvent")]
        public async Task<IActionResult> updateEvent(
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
            Events Event = JsonConvert.DeserializeObject<Events>(requestBody);

            // ValiEvent the object
            if (!IsValidEvent(Event))
            {
                return new BadRequestObjectResult("Invalid Event data ");
            }
            Event.Date = Event.Date.ToUniversalTime();//convert to UTC

            var Eventfromdb = await _EventsRepository.GetEventAsync(Event.Id);
            if (Eventfromdb == null)
            {
                return new NotFoundObjectResult("Event not found.");
            }
            Eventfromdb.userid = Event.userid;
            Eventfromdb.Title = Event.Title;
            Eventfromdb.Date = Event.Date;
            Eventfromdb.Description = Event.Description;
            Eventfromdb.MarkFinished = Event.MarkFinished;
           
            Eventfromdb.Id = Event.Id;

            await _EventsRepository.UpdateEventAsync(Event.Id, Eventfromdb);

            return new OkObjectResult(new { message = "Event updated successfully" });
        }


        [FunctionName("EventDelete")]
        public async Task<IActionResult> EventDelete(
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

            await _EventsRepository.DeleteEventAsync(id);

            return new OkObjectResult(new { message = "Event deleted successfully" });
        }

        [FunctionName("EventGet")]      
        public async Task<IActionResult> EventGet(
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
                return new BadRequestObjectResult("Event Id is required.");
            }

            var Event = await _EventsRepository.GetEventAsync(id);

            return new OkObjectResult(Event);
        }

    }
}

