using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;
using Testcontainers.Redis;
using Testcontainers.RabbitMq;
using Xunit;

namespace Fin_Backend.Tests.Common
{
    public class TestContainerFixture : IAsyncLifetime
    {
        public MsSqlContainer SqlContainer { get; }
        public RedisContainer RedisContainer { get; }
        public RabbitMqContainer RabbitMqContainer { get; }

        public TestContainerFixture()
        {
            // Create SQL Server container
            SqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("StrongP@ssw0rd!")
                .WithCleanUp(true)
                .Build();

            // Create Redis container
            RedisContainer = new RedisBuilder()
                .WithImage("redis:7.0-alpine")
                .WithCleanUp(true)
                .Build();

            // Create RabbitMQ container
            RabbitMqContainer = new RabbitMqBuilder()
                .WithImage("rabbitmq:3.11-management-alpine")
                .WithCleanUp(true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            // Start containers
            await SqlContainer.StartAsync();
            await RedisContainer.StartAsync();
            await RabbitMqContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            // Stop containers
            await SqlContainer.StopAsync();
            await RedisContainer.StopAsync();
            await RabbitMqContainer.StopAsync();
        }
    }

    public class TestContainerWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>, IAsyncLifetime where TStartup : class
    {
        private readonly TestContainerFixture _fixture;
        
        public TestContainerWebApplicationFactory()
        {
            _fixture = new TestContainerFixture();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Override connection strings with container connection strings
                services.Configure<ConnectionStrings>(options =>
                {
                    options.DefaultConnection = _fixture.SqlContainer.GetConnectionString();
                    options.Redis = _fixture.RedisContainer.GetConnectionString();
                });

                // Override RabbitMQ settings
                services.Configure<RabbitMQSettings>(options =>
                {
                    options.Host = _fixture.RabbitMqContainer.Hostname;
                    options.Port = _fixture.RabbitMqContainer.GetMappedPort(5672);
                    options.Username = "guest";
                    options.Password = "guest";
                    options.VirtualHost = "/";
                });
            });

            builder.UseEnvironment("Test");
        }

        public async Task InitializeAsync()
        {
            await _fixture.InitializeAsync();
        }

        public new async Task DisposeAsync()
        {
            await _fixture.DisposeAsync();
            base.Dispose();
        }
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
        public string Redis { get; set; }
    }

    public class RabbitMQSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
    }
}