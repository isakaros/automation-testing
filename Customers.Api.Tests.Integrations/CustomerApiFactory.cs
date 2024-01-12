using Customers.Api.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Testcontainers.PostgreSql;
using Xunit;

namespace Customers.Api.Tests.Integrations
{
    public class CustomerApiFactory: WebApplicationFactory<IApiMarker>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer =
            new PostgreSqlBuilder()
            .WithDatabase("mydb")
            .WithUsername("workshop")
            .WithPassword("changeme")
            .Build();

        private readonly GitHubApiServer _gitHubApiServer = new ();
        public const string ValidGithubUser = "validuser";
        public const string ThrottledGithubUser = "throttleduser";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(IHostedService));
                services.RemoveAll(typeof(IDbConnectionFactory));
                services.AddSingleton<IDbConnectionFactory>(_ =>
                new NpgsqlConnectionFactory(_dbContainer.GetConnectionString()));

                services.AddHttpClient("GitHub", httpClient =>
                {
                    httpClient.BaseAddress = new Uri(_gitHubApiServer.Url);
                    httpClient.DefaultRequestHeaders.Add(
                        HeaderNames.Accept, "application/vnd.github.v3+json");
                    httpClient.DefaultRequestHeaders.Add(
                        HeaderNames.UserAgent, $"Course-{Environment.MachineName}");
                });
            });
        }

        public async Task InitializeAsync()
        {
            _gitHubApiServer.Start();
            _gitHubApiServer.SetupUser(ValidGithubUser);
            _gitHubApiServer.SetupThrottledUser(ThrottledGithubUser);
            await _dbContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
            _gitHubApiServer.Dispose();
        }
    }
}
