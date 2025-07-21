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
    public class TaskManager
    {
        private readonly ILogger<TaskManager> _logger;
        private readonly TaskRepository _taskRepository ;

        public TaskManager(ILogger<TaskManager> log, TaskRepository taskRepository)
        {
            _logger = log;
            _taskRepository= taskRepository;
        }

        [FunctionName("createTaskEntry")]
        public async Task<IActionResult> CreateTaskEntry(
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

           Models.Task task= JsonConvert.DeserializeObject<Models.Task>(requestBody);

            if (IsValidEntry(task) == false)
            {
                return new BadRequestObjectResult("Invalid task entry data.");
            }
            task.Date = task.Date.ToUniversalTime();
            await _taskRepository.CreateTaskEntryAsync(task);

            return new OkObjectResult(new { message = "Task Entry added successfully", data = task });
        }

        private bool IsValidEntry(Models.Task tra)
        {
            return tra!= null &&
                   !string.IsNullOrWhiteSpace(tra.Title)                    
                   ;
        }

        [FunctionName("GetTaskEntries")]
        public async Task<IActionResult> GetTaskEntries(
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

            var result = await _taskRepository.GetAllTasksEntriesAsync(userId, searchText, pageNumber);

            return result;
        }

        [FunctionName("updateTaskEntry")]
        public async Task<IActionResult> updateTaskEntry(
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
            Models.Task task= JsonConvert.DeserializeObject<Models.Task>(requestBody);

            // Validate the object
            if (!IsValidEntry(task))
            {
                return new BadRequestObjectResult("Invalid task data ");
            }
            task.Date = task.Date.ToUniversalTime();
            var taskfromdb = await _taskRepository.GetTaskEntryAsync(task.Id);

            if (taskfromdb== null)
            {
                return new NotFoundObjectResult("task not found.");
            }
            taskfromdb.UserId =task.UserId;
            taskfromdb.Title = task.Title;
            taskfromdb.Date = task.Date;
            taskfromdb.Status= task.Status;
            taskfromdb.UserId= task.UserId;
            taskfromdb.Description = task.Description;
            taskfromdb.Id = task.Id;

            await _taskRepository.UpdateTaskEntryAsync(task.Id, taskfromdb);

            return new OkObjectResult(new { message = "Task Entry updated successfully" });
        }

        [FunctionName("deleteTask")]
        public async Task<IActionResult> taskDelete(
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

            await _taskRepository.DeleteTaskEntryAsync(id);

            return new OkObjectResult(new { message = "Task Entry deleted successfully" });
        }

        [FunctionName("getTask")]
        public async Task<IActionResult> taskGet(
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
                return new BadRequestObjectResult("task Id is required.");
            }

            var tra = await _taskRepository.GetTaskEntryAsync(id);
            
            return new OkObjectResult(tra);
        }
    }
}

