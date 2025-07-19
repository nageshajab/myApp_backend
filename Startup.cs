using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using myazfunction.DAL;

[assembly: FunctionsStartup(typeof(myazfunction.Startup))]

namespace myazfunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<MongoDbSettings>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("MongoDb").Bind(settings);
                });

            builder.Services.AddSingleton<MongoDbSettings>(sp =>
            {
                var options = sp.GetService<IOptions<MongoDbSettings>>();
                return options.Value;
            });

            builder.Services.AddSingleton<MongoDbContext>();
            builder.Services.AddSingleton<UserRepository>();
            builder.Services.AddSingleton<DatesRepository>();
            builder.Services.AddSingleton<KhataRepository>();
        }
    }
}

