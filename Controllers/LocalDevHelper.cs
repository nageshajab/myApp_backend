using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using myazfunction.DAL;
using myazfunction.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace myazfunction.Controllers
{
    public class LocalDevHelper
    {
        private string userid = "687641500069d713a242e626";
        private string newuserid = "1fcca2c1-ffda-4cc5-b5bd-8959dec8d5af";

        private readonly DatesRepository _datesRepository;
        private readonly KhataRepository _khataRepository;
        private readonly PasswordRepository _passwordRepository;
        private readonly RentRepository _rentRepository;
        private readonly TaskRepository _taskRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly UserRepository _userRepository;
        private readonly WatchlistRepository _watchlistRepository;
        private readonly EventsRepository _eventsRepository;

        public LocalDevHelper(DatesRepository datesRepository, KhataRepository khataRepository, PasswordRepository passwordRepository, RentRepository rentRepository, TaskRepository taskRepository, TenantRepository tenantRepository, TransactionRepository transactionRepository, UserRepository userRepository, WatchlistRepository watchlistRepository, EventsRepository eventsRepository)
        {
            _datesRepository = datesRepository;
            _khataRepository = khataRepository;
            _passwordRepository = passwordRepository;
            _rentRepository = rentRepository;
            _taskRepository = taskRepository;
            _tenantRepository = tenantRepository;
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _watchlistRepository = watchlistRepository;
            _eventsRepository = eventsRepository;
        }

        [FunctionName("execute")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> execute(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            await copyDatesToEvents();
            //await copyDates();
            //Console.WriteLine("Dates copied..");
            //// await copyKhataData();
            //Console.WriteLine("Khata data copied..");
            //// await copyPasswordData();
            //Console.WriteLine("Password data copied..");
            //await copyRentData();
            //Console.WriteLine("rent data copied");
            //await copyTaskData();
            //Console.WriteLine("Task data copied..");
            //await copyTenantData();
            //Console.WriteLine("tenant data copied..");
            //await copyTransactionData();
            //Console.WriteLine("Transaction data copied..");
            //await copyWatchlistData();
            //Console.WriteLine("Watchlist data copied..");

            return new OkObjectResult("ok");
        }

        private async System.Threading.Tasks.Task copyDatesToEvents()
        {
            //copy dates from one user to another
            var documents = await _datesRepository.GetAllDatesAsync(newuserid);
            foreach (var date in documents)
            {
                var newEvent = new Events
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    userid = date.userid,
                    Title = date.Title,
                    Date = date.Date,
                    Description = date.Description,
                    MarkFinished=false
                };

                await _eventsRepository.CreateEventAsync(newEvent);
            }
        }

        private async System.Threading.Tasks.Task copyDates()
        {
            //copy dates from one user to another
            var documents = await _datesRepository.GetAllDatesAsync(userid);
            foreach (var date in documents)
            {
                var newDate = new Dates
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    userid = newuserid,
                    Title = date.Title,
                    Date = date.Date,
                    Description = date.Description,
                    Duration = date.Duration,
                    isRecurring = date.isRecurring,
                    RecurringEvent = date.RecurringEvent
                };

                await _datesRepository.CreateDateAsync(newDate);
            }
        }

        private async System.Threading.Tasks.Task copyKhataData()
        {
            //copy dates from one user to another
            var documents = await _khataRepository.GetAllKhataEntriesAsync(userid);

            foreach (var d in documents)
            {
                Khata khata = new Khata()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = newuserid,
                    Title = d.Title,
                    Amount = d.Amount,
                    Date = d.Date,
                    PersonName = d.PersonName
                };

                await _khataRepository.CreateKhataEntryAsync(khata);
            }
        }

        private async System.Threading.Tasks.Task copyPasswordData()
        {
            //copy dates from one user to another
            var documents = await _passwordRepository.GetAllPasswordsAsync(userid);

            foreach (var d in documents)
            {
                Passwords p = new Passwords
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = newuserid,
                    Password = d.Password,
                    System = d.System,
                    UserName = d.UserName,
                };

                await _passwordRepository.CreatePasswordAsync(p);
            }
        }

        private async System.Threading.Tasks.Task copyRentData()
        {
            //copy dates from one user to another
            var documents = await _rentRepository.GetAllRentsAsync(userid);

            foreach (var d in documents)
            {
                Rent p = new Rent
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = newuserid,
                    Date = d.Date,
                    Mseb = d.Mseb,
                    PaidAmount = d.PaidAmount,
                    RemainingAmount = d.RemainingAmount,
                    TenantName = d.TenantName,
                };

                await _rentRepository.CreateRentAsync(p);
            }
        }

        private async System.Threading.Tasks.Task copyTaskData()
        {
            //copy dates from one user to another
            var documents = await _taskRepository.GetAllTasksEntriesAsync(userid);

            foreach (var d in documents)
            {
                Models.Task p = new Models.Task
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = newuserid,
                    Date = d.Date,
                    Description = d.Description,
                    Status = d.Status,
                    Title = d.Title,
                };

                await _taskRepository.CreateTaskEntryAsync(p);
            }
        }

        private async System.Threading.Tasks.Task copyTenantData()
        {
            //copy dates from one user to another
            var documents = await _tenantRepository.GetAllTenantsAsync(userid);

            foreach (var d in documents)
            {
                Tenant p = new Tenant
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = newuserid,
                    Description = d.Description,
                    Deposit = d.Deposit,
                    EndDate = d.EndDate,
                    IsActive = d.IsActive,
                    Rent = d.Rent,
                    StartDate = d.StartDate,
                    TenantName = d.TenantName
                };

                await _tenantRepository.CreateTenantAsync(p);
            }
        }

        private async System.Threading.Tasks.Task copyTransactionData()
        {
            //copy dates from one user to another
            var documents = await _transactionRepository.GetAllTransactionsEntriesAsync(userid);

            foreach (var d in documents)
            {
                Transactions p = new Transactions
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = newuserid,
                    Amount = d.Amount,
                    Date = d.Date,
                    Description = d.Description,
                    Title = d.Title,
                };
                await _transactionRepository.CreateTransactionEntryAsync(p);
            }
        }

        private async System.Threading.Tasks.Task copyUserData()
        {
            //copy dates from one user to another
            var documents = await _userRepository.GetAllUsersAsync();

            foreach (var d in documents)
            {
                Users p = new Users
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Email = d.Email,
                    UserName = d.UserName,
                    Isverified = d.Isverified,
                    Password = d.Password
                };
                await _userRepository.CreateUserAsync(p);
            }
        }

        private async System.Threading.Tasks.Task copyWatchlistData()
        {
            //copy dates from one user to another
            var documents = await _watchlistRepository.GetAllWatchlistEntriesAsync(userid);

            foreach (var d in documents)
            {
                Watchlist p = new Watchlist
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Date = d.Date,
                    Title = d.Title,
                    Genre = d.Genre,
                    UserId = newuserid,
                    Language = d.Language,
                    Status = d.Status,
                    Type = d.Type,
                    Ott = d.Ott,
                    Rating = d.Rating,

                };
                await _watchlistRepository.CreateWatchlistEntryAsync(p);
            }
        }
    }
}
