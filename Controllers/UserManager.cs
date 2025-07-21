using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using myazfunction.DAL;
using myazfunction.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace myazfunction.Controllers
{
    public class UserManager
    {
        private readonly ILogger<PasswordManager> _logger;
        private readonly UserRepository _userRepository;

        public UserManager(ILogger<PasswordManager> log, UserRepository userRepository)
        {
            _logger = log;
            _userRepository = userRepository;
        }

        [FunctionName("register")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "UserId" })]
        [OpenApiParameter(name: "UserId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **UserId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> register(
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

            Users newUser = JsonConvert.DeserializeObject<Users>(requestBody);

            if (IsValidUser(newUser) == false)
            {
                return new BadRequestObjectResult("Invalid password data.");
            }
            
            await _userRepository.CreateUserAsync(newUser);

            return new OkObjectResult(new { message = "Password added successfully", data = newUser });
        }

        private bool IsValidUser(Users users)
        {
            return users != null &&
                   !string.IsNullOrWhiteSpace(users.Email) &&
                   !string.IsNullOrWhiteSpace(users.UserName) &&
                   !string.IsNullOrWhiteSpace(users.Password);
        }

        [FunctionName("GetUsers")]
        public async Task<IActionResult> GetUsers(
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
            string userId = req.Query["UserId"];
            string searchText = req.Query["searchText"];
            int pageNumber = int.TryParse(req.Query["pageNumber"], out var pn) ? pn : 1;
            int pageSize = int.TryParse(req.Query["pageSize"], out var ps) ? ps : 10;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            userId = userId ?? data?.UserId;
            searchText = searchText ?? data?.searchText;

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _userRepository.GetAllUsersAsync();

            return new OkObjectResult(result);
        }

        [FunctionName("ChangePassword")]
        public async Task<IActionResult> UserUpdate(
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
            ChangePassword changePassword= JsonConvert.DeserializeObject<ChangePassword>(requestBody);

            // Validate the object
            if (!IsValidChangePassword(changePassword))
            {
                return new BadRequestObjectResult("Invalid change password data ");
            }
            var user= await _userRepository.GetUserAsync(changePassword.Id);
            if (user == null)
            {
                return new NotFoundObjectResult("User not found.");
            }
            else
            {
                if (!BCrypt.Net.BCrypt.Verify(changePassword.currentPassword, user.Password))
                {
                    return new BadRequestObjectResult("Current password is incorrect.");
                }
            }
            user.Password = changePassword.password;
            await _userRepository.UpdateUserAsync(user.Id, user);

            return new OkObjectResult(new { message = "Password updated successfully" });

        }

        private bool IsValidChangePassword(ChangePassword cp)
        {
            return cp!= null &&
                 !string.IsNullOrWhiteSpace(cp.UserName) &&
                 !string.IsNullOrWhiteSpace(cp.currentPassword) &&
                 !string.IsNullOrWhiteSpace(cp.Id) &&
                 !string.IsNullOrWhiteSpace(cp.password);
        }

        [FunctionName("UserDelete")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> UserDelete(
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

            await _userRepository.DeleteUserAsync(id);

            return new OkObjectResult(new { message = "Password deleted successfully" });

        }

        [FunctionName("UserGet")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> UserGet(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
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

            var password = await _userRepository.GetUserAsync(id);
            return new OkObjectResult(password);

        }

        [FunctionName("Login")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Login(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
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
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var loginRequest = JsonConvert.DeserializeObject<Users>(requestBody);

            if (string.IsNullOrEmpty(loginRequest.UserName) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return new BadRequestObjectResult("Username or password is empty");
            }


            var user = await _userRepository.LoginAsync(loginRequest.UserName, loginRequest.Password);

            if (user == null)
            {
                return new UnauthorizedResult();
            }

            return new OkObjectResult(new { message = "Login successful", user });

        }
    }
}

