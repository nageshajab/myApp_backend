using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.DAL;
using myazfunction.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace myazfunction.Controllers
{
    public class PasswordsPage
    {
        public int pageNumber { get; set; }
        public string searchtxt { get; set; }
        public string userid { get; set; }
    }

    public class PasswordManager
    {
        private readonly ILogger<PasswordManager> _logger;
        private readonly PasswordRepository _passwordRepository;


        public PasswordManager(ILogger<PasswordManager> log, PasswordRepository passwordRepository)
        {
            _logger = log;
            _passwordRepository = passwordRepository;
        }

        [FunctionName("PasswordCreate")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "UserId" })]
        [OpenApiParameter(name: "UserId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **UserId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> PasswordCreate(
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

            Passwords newPassword = JsonConvert.DeserializeObject<Passwords>(requestBody);

            if (IsValidPassword(newPassword) == false)
            {
                return new BadRequestObjectResult("Invalid password data.");
            }

            await _passwordRepository.CreatePasswordAsync(newPassword);

            return new OkObjectResult(new { message = "Password added successfully", data = newPassword });
        }

        private bool IsValidPassword(Passwords password)
        {
            return password != null &&
                   !string.IsNullOrWhiteSpace(password.System) &&
                   !string.IsNullOrWhiteSpace(password.UserName) &&
                   !string.IsNullOrWhiteSpace(password.Password) &&
                   !string.IsNullOrWhiteSpace(password.UserId);
        }

        [FunctionName("GetPasswords")]
        public async Task<IActionResult> GetPasswords(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
       ILogger log)
        {
            int pageSize = 10;
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
            PasswordsPage data = JsonConvert.DeserializeObject<PasswordsPage>(requestBody);

            if (string.IsNullOrEmpty(data.userid))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _passwordRepository.GetAllPasswordsAsync(data.userid, data.searchtxt, data.pageNumber);

            return new OkObjectResult(result);
        }


        [FunctionName("PasswordUpdate")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> PasswordUpdate(
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
            Passwords passwords = JsonConvert.DeserializeObject<Passwords>(requestBody);

            // Validate the object
            if (!IsValidPassword(passwords) || string.IsNullOrEmpty(passwords.Id))
            {
                return new BadRequestObjectResult("Invalid password data or missing Id.");
            }

            await _passwordRepository.UpdatePasswordAsync(passwords.Id, passwords);

            return new OkObjectResult(new { message = "Password updated successfully", data = passwords });

        }

        [FunctionName("PasswordDelete")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> PasswordDelete(
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

            await _passwordRepository.DeletePasswordAsync(id);

            return new OkObjectResult(new { message = "Password deleted successfully", deletedId = id });

        }

        [FunctionName("PasswordGet")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> PasswordGet(
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
                return new BadRequestObjectResult("Password Id is required.");
            }

            var password = await _passwordRepository.GetPasswordAsync(id);

            return new OkObjectResult(password);
        }
    }
}

