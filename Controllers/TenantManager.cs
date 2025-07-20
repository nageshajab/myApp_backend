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
    public class TenantManager
    {
        private readonly ILogger<TenantManager> _logger;
        private readonly TenantRepository _tenantRepository;

        public TenantManager(ILogger<TenantManager> log, TenantRepository tenantRepository)
        {
            _logger = log;
            _tenantRepository=tenantRepository;
        }

        [FunctionName("createTenant")]
        public async Task<IActionResult> createTenant(
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

           Tenant tenant = JsonConvert.DeserializeObject<Tenant>(requestBody);

            if (IsValidEntry(tenant) == false)
            {
                return new BadRequestObjectResult("Invalid tenant data.");
            }

            await _tenantRepository.CreateTenantAsync(tenant);

            return new OkObjectResult(new { message = "tenant added successfully", data = tenant });
        }

        private bool IsValidEntry(Tenant tenant)
        {
            return tenant != null &&
                   !string.IsNullOrWhiteSpace(tenant.TenantName);
        }

        [FunctionName("GetTenants")]
        public async Task<IActionResult> GetTenants(
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

            var result = await _tenantRepository.GetAllTenantsAsync(userId, searchText, pageNumber);

            return result;
        }

        [FunctionName("updateTenant")]
        public async Task<IActionResult> updateTenant(
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
            Tenant tenant= JsonConvert.DeserializeObject<Tenant>(requestBody);

            // Validate the object
            if (!IsValidEntry(tenant))
            {
                return new BadRequestObjectResult("Invalid tenant data");
            }

            var tenantfromdb = await _tenantRepository.GetTenantAsync(tenant.Id);

            if (tenantfromdb    == null)
            {
                return new NotFoundObjectResult("khata not found.");
            }
            tenantfromdb.Id = tenant.Id;
            tenantfromdb.StartDate = tenant.StartDate;
            tenantfromdb.EndDate = tenant.EndDate;
            tenantfromdb.Deposit = tenant.Deposit;
            tenantfromdb.UserId = tenant.UserId;
            tenantfromdb.TenantName = tenant.TenantName;

            await _tenantRepository.UpdateTenantAsync(tenant.Id, tenantfromdb);

            return new OkObjectResult(new { message = "Tenant updated successfully" });
        }

        [FunctionName("DeleteTenant")]
        public async Task<IActionResult> DeleteTenant(
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

            await _tenantRepository.DeleteTenantAsync(id);

            return new OkObjectResult(new { message = "Tenant deleted successfully" });
        }

        [FunctionName("GetTenant")]
        public async Task<IActionResult> GetTenant(
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
                return new BadRequestObjectResult("tenant Id is required.");
            }

            var khata = await _tenantRepository.GetTenantAsync(id);
            
            return new OkObjectResult(khata);
        }
    }
}

