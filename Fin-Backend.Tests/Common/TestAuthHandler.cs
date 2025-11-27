using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinTech.WebAPI.Application.DTOs.Auth;
using FinTech.WebAPI.Application.DTOs.Common;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Tests.Common
{
    public static class TestAuthHandler
    {
        public static async Task<string> GetJwtTokenAsync(HttpClient client)
        {
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Test@123"
            };

            var response = await client.PostAsJsonAsync("/api/Auth/login", loginRequest);
            response.EnsureSuccessStatusCode();

            var loginResponse = await response.Content.ReadFromJsonAsync<BaseResponse<LoginResponse>>();
            
            return loginResponse?.Data?.Token ?? throw new InvalidOperationException("Failed to get token");
        }
        
        public static async Task<HttpClient> GetAuthenticatedClientAsync(TestWebApplicationFactory<Program> factory)
        {
            var client = factory.CreateClient();
            var token = await GetJwtTokenAsync(client);
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            return client;
        }
    }
}