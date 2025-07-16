using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;

namespace myazfunction
{
    public class MyEmailAddress
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class EmailManager
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<PasswordManager> _logger;
       
        public EmailManager(ILogger<PasswordManager> log, IConfiguration configuration)
        {
            _logger = log;
            _configuration = configuration;
        }

        private bool IsValidEmail(MyEmailAddress email)
        {
            if (email == null || string.IsNullOrWhiteSpace(email.ToEmail) || string.IsNullOrWhiteSpace(email.Subject) || string.IsNullOrWhiteSpace(email.Body))
            {
                return false;
            }
            return true;
        }

        [FunctionName("SendEmail")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "UserId" })]
        [OpenApiParameter(name: "UserId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **UserId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> PasswordCreate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string UserId = req.Query["UserId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            MyEmailAddress email = JsonConvert.DeserializeObject<MyEmailAddress>(requestBody);

            if(IsValidEmail(email) == false)
            {
                return new BadRequestObjectResult("Invalid email data.");
            }        

            string connectionString = _configuration["email"];            
            var emailClient = new EmailClient(connectionString);

            var emailMessage = new EmailMessage(
                senderAddress: "DoNotReply@77306963-2ba1-4817-aaf3-7386dff7ddbe.azurecomm.net",
                content: new EmailContent("Test Email")
                {
                    PlainText = email.Body,
                    Html = string.Format("<html><body><h1>{0}</h1></body></html>",email.Body)
                },
                recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(email.ToEmail) }));

            EmailSendOperation emailSendOperation = emailClient.Send(
                WaitUntil.Completed,
                emailMessage);

            return new OkObjectResult(new { message = "Email sent" });
        }

      
    }
}

