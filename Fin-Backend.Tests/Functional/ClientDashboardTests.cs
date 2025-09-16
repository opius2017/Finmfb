using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Fin_Backend.Tests.Common;
using FinTech.WebAPI.Application.DTOs.ClientPortal;
using FinTech.WebAPI.Application.DTOs.Common;
using FluentAssertions;
using Xunit;

namespace Fin_Backend.Tests.Functional
{
    public class ClientDashboardTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ClientDashboardTests(TestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetDashboardData_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/ClientDashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetDashboardData_AuthenticatedUser_ReturnsDashboardData()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);

            // Act
            var response = await client.GetAsync("/api/ClientDashboard");

            // Assert
            response.EnsureSuccessStatusCode();
            var dashboardResponse = await response.Content.ReadFromJsonAsync<BaseResponse<DashboardDto>>();
            
            dashboardResponse.Should().NotBeNull();
            dashboardResponse.Success.Should().BeTrue();
            dashboardResponse.Data.Should().NotBeNull();
            dashboardResponse.Data.Accounts.Should().NotBeNull();
            dashboardResponse.Data.RecentTransactions.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAccounts_AuthenticatedUser_ReturnsAccounts()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);

            // Act
            var response = await client.GetAsync("/api/ClientDashboard/accounts");

            // Assert
            response.EnsureSuccessStatusCode();
            var accountsResponse = await response.Content.ReadFromJsonAsync<BaseResponse<AccountsDto>>();
            
            accountsResponse.Should().NotBeNull();
            accountsResponse.Success.Should().BeTrue();
            accountsResponse.Data.Should().NotBeNull();
            accountsResponse.Data.Accounts.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAccountDetails_ValidAccountId_ReturnsAccountDetails()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);
            
            // First get all accounts
            var accountsResponse = await client.GetAsync("/api/ClientDashboard/accounts");
            accountsResponse.EnsureSuccessStatusCode();
            var accounts = await accountsResponse.Content.ReadFromJsonAsync<BaseResponse<AccountsDto>>();
            
            // Make sure there's at least one account
            accounts.Data.Accounts.Count.Should().BeGreaterThan(0, "Test user should have at least one account");
            
            var accountId = accounts.Data.Accounts[0].AccountId;

            // Act
            var response = await client.GetAsync($"/api/ClientDashboard/accounts/{accountId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var accountDetailsResponse = await response.Content.ReadFromJsonAsync<BaseResponse<AccountDetailsDto>>();
            
            accountDetailsResponse.Should().NotBeNull();
            accountDetailsResponse.Success.Should().BeTrue();
            accountDetailsResponse.Data.Should().NotBeNull();
            accountDetailsResponse.Data.AccountId.Should().Be(accountId);
            accountDetailsResponse.Data.AccountNumber.Should().NotBeNullOrEmpty();
            accountDetailsResponse.Data.Balance.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public async Task GetAccountTransactions_ValidAccountId_ReturnsTransactions()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);
            
            // First get all accounts
            var accountsResponse = await client.GetAsync("/api/ClientDashboard/accounts");
            accountsResponse.EnsureSuccessStatusCode();
            var accounts = await accountsResponse.Content.ReadFromJsonAsync<BaseResponse<AccountsDto>>();
            
            // Make sure there's at least one account
            accounts.Data.Accounts.Count.Should().BeGreaterThan(0, "Test user should have at least one account");
            
            var accountId = accounts.Data.Accounts[0].AccountId;

            // Act
            var response = await client.GetAsync($"/api/ClientDashboard/accounts/{accountId}/transactions?page=1&pageSize=10");

            // Assert
            response.EnsureSuccessStatusCode();
            var transactionsResponse = await response.Content.ReadFromJsonAsync<BaseResponse<PagedResponse<TransactionDto>>>();
            
            transactionsResponse.Should().NotBeNull();
            transactionsResponse.Success.Should().BeTrue();
            transactionsResponse.Data.Should().NotBeNull();
            transactionsResponse.Data.Items.Should().NotBeNull();
            transactionsResponse.Data.CurrentPage.Should().Be(1);
            transactionsResponse.Data.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetAccountStatement_ValidParameters_ReturnsStatement()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);
            
            // First get all accounts
            var accountsResponse = await client.GetAsync("/api/ClientDashboard/accounts");
            accountsResponse.EnsureSuccessStatusCode();
            var accounts = await accountsResponse.Content.ReadFromJsonAsync<BaseResponse<AccountsDto>>();
            
            // Make sure there's at least one account
            accounts.Data.Accounts.Count.Should().BeGreaterThan(0, "Test user should have at least one account");
            
            var accountId = accounts.Data.Accounts[0].AccountId;
            
            var startDate = System.DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
            var endDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd");

            // Act
            var response = await client.GetAsync($"/api/ClientDashboard/accounts/{accountId}/statement?startDate={startDate}&endDate={endDate}&format=json");

            // Assert
            response.EnsureSuccessStatusCode();
            var statementResponse = await response.Content.ReadFromJsonAsync<BaseResponse<AccountStatementDto>>();
            
            statementResponse.Should().NotBeNull();
            statementResponse.Success.Should().BeTrue();
            statementResponse.Data.Should().NotBeNull();
            statementResponse.Data.AccountDetails.Should().NotBeNull();
            statementResponse.Data.AccountDetails.AccountId.Should().Be(accountId);
            statementResponse.Data.Transactions.Should().NotBeNull();
            statementResponse.Data.StatementPeriod.Should().NotBeNullOrEmpty();
        }
    }
}