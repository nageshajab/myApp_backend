using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using myazfunction.DAL;
using myazfunction.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace myazfunction.Controllers
{
    public class MovieManager
    {
        private readonly ILogger<WatchlistManager> _logger;
        private readonly MovieRepository _movieRepository;

        public MovieManager(ILogger<WatchlistManager> log, MovieRepository movieRepository)
        {
            _logger = log;
            _movieRepository = movieRepository;
        }

        [FunctionName("createmovie")]
        public async Task<IActionResult> createmovie(
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

           Movie movie= JsonConvert.DeserializeObject<Movie>(requestBody);

            if (IsValid(movie) == false)
            {
                return new BadRequestObjectResult("Invalid movie data.");
            }
            
            await _movieRepository.CreateMovieAsync(movie);

            return new OkObjectResult(new { message = "movie added successfully", data = movie});
        }

        private bool IsValid(Movie tra)
        {
            return tra != null &&
                   !string.IsNullOrWhiteSpace(tra.Title) &&
                   tra.tags.Length!=0                 
                  ;
        }

        [FunctionName("GetAllTags")]
        public async Task<IActionResult> GetAllTags(
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

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _movieRepository.GetAllTagsAsync(userId);

            return new OkObjectResult( result);
        }

        [FunctionName("GetMovies")]
        public async Task<IActionResult> GetMovies(
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
            string selectedtags = data?.tags;
            bool isJav = data?.isJav;
            int pageNumber = data?.pageNumber;
            pageNumber = pageNumber > 0 ? pageNumber : 1;

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult("UserId is required.");
            }

            var result = await _movieRepository.GetAllMoviesAsync(userId, searchText,selectedtags, pageNumber,isJav);

            return result;
        }

        [FunctionName("updateMovie")]
        public async Task<IActionResult> updateMovie(
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
            Movie movie= JsonConvert.DeserializeObject<Movie>(requestBody);

            // Validate the object
            if (!IsValid(movie))
            {
                return new BadRequestObjectResult("Invalid movie item");
            }

            var moviefromdb = await _movieRepository.GetMovieAsync(movie.Id);

            if (moviefromdb== null)
            {
                return new NotFoundObjectResult("movie not found.");
            }
            moviefromdb.Id = movie.Id;
            moviefromdb.Title = movie.Title;
            moviefromdb.tags= movie.tags;
            moviefromdb.UserId = movie.UserId;
            moviefromdb.ImageData=movie.ImageData;
            moviefromdb.Url = movie.Url;
            moviefromdb.IsJav = movie.IsJav;

            await _movieRepository.UpdateMovieAsync(movie.Id, moviefromdb);

            return new OkObjectResult(new { message = "movie updated successfully" });
        }

        [FunctionName("DeleteMovie")]
        public async Task<IActionResult> DeleteMovie(
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

            await _movieRepository.DeleteMovieAsync(id);

            return new OkObjectResult(new { message = "movie deleted successfully" });
        }

        [FunctionName("getMovie")]
        public async Task<IActionResult> getMovie(
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
                return new BadRequestObjectResult("movie Id is required.");
            }

            var tra = await _movieRepository.GetMovieAsync(id);
            
            return new OkObjectResult(tra);
        }

        [FunctionName("GetImage")]
        public async Task<IActionResult> GetImage(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "image/{id}")] HttpRequest req,
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

            var imageBytes =await _movieRepository.GetImage(id);
            return new FileContentResult(imageBytes, "image/jpeg"); // or "image/png"
        }
    }
}

