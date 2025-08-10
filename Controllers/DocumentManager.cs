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
    public class DocumentsPage
    {
        public int pageNumber { get; set; }
        public string searchtxt { get; set; }
        public string userid { get; set; }
    }

    public class DocumentManager
    {
        private readonly ILogger<PasswordManager> _logger;
        private readonly DocumentRepository _documentRepository;


        public DocumentManager(ILogger<PasswordManager> log, DocumentRepository documentRepository)
        {
            _logger = log;
            _documentRepository = documentRepository;
        }

        [FunctionName("DocumentCreate")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "UserId" })]
        [OpenApiParameter(name: "UserId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **UserId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> DocumentCreate(
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

            Document document = JsonConvert.DeserializeObject<Document>(requestBody);

            if (IsValidDocument(document) == false)
            {
                return new BadRequestObjectResult("Invalid document data.");
            }

            await _documentRepository.CreateDocumentAsync(document);

            return new OkObjectResult(new { message = "Document added successfully", data = document });
        }

        private bool IsValidDocument(Document document)
        {
            return document != null &&
                   !string.IsNullOrWhiteSpace(document.Title) &&
                   (!string.IsNullOrWhiteSpace(document.Url)
                   || document.File.Length != 0
                   ) &&
            !string.IsNullOrWhiteSpace(document.UserId);
        }

        [FunctionName("GetDocuments")]
        public async Task<IActionResult> GetDocuments(
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
            DocumentsPage data = JsonConvert.DeserializeObject<DocumentsPage>(requestBody);

            if (string.IsNullOrEmpty(data.userid))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _documentRepository.GetAllDocumentsAsync(data.userid, data.searchtxt, data.pageNumber);

            return result;
        }


        [FunctionName("DocumentUpdate")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> DocumentUpdate(
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
            Document document = JsonConvert.DeserializeObject<Document>(requestBody);

            // Validate the object
            if (!IsValidDocument(document) || string.IsNullOrEmpty(document.Id))
            {
                return new BadRequestObjectResult("Invalid document data or missing Id.");
            }

            await _documentRepository.UpdateDocumentAsync(document.Id, document);

            return new OkObjectResult(new { message = "Document updated successfully", data = document });

        }

        [FunctionName("DocumentDelete")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> DocumentDelete(
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
                return new BadRequestObjectResult("document Id is required.");
            }

            await _documentRepository.DeleteDocumentAsync(id);

            return new OkObjectResult(new { message = "document deleted successfully", deletedId = id });

        }

        [FunctionName("DocumentGet")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> DocumentGet(
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

            var document = await _documentRepository.GetDocumentAsync(id);

            return new OkObjectResult(document);
        }

        [FunctionName("GetDocumentBytes")]
        public async Task<IActionResult> GetDocumentBytes(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "document/{id}")] HttpRequest req,
string id,
ILogger log)
        {
            // Set CORS headers on the response
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            // Handle preflight OPTIONS request
            if (req.Method == HttpMethods.Options)
            {
                return new OkResult(); // No body needed for preflight
            }

            var item = await _documentRepository.GetDocumentBytes(id);
            if (item == null || item.File == null)
            {
                return new NotFoundResult();
            }

            var fileName = string.IsNullOrEmpty(item.FileName) ? "document.bin" : item.FileName;
            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = fileName,
                Inline = false
            };

            req.HttpContext.Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

            return new FileContentResult(item.File, "application/octet-stream");
        }
    }
}

