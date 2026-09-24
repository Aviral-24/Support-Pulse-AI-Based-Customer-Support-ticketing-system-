using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Unit_Tests
{
    // Scheme ka naam 'TestScheme' rakha hai taaki 'Bearer' ke sath clash/crash na ho
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) 
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "999"), 
                new Claim(ClaimTypes.Role, "Customer")       // Bypass test ke liye role 'Customer' hona zaroori hai
            };
            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class UnitIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UnitIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestScheme";
                        options.DefaultChallengeScheme = "TestScheme";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
                });
            }).CreateClient();
        }

        private void AuthenticateAsCustomer()
        {
            // 'Bearer' ki jagah 'TestScheme' use kar rahe hain
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme", "mock_token");
        }

        // EXACTLY 4 REQUIRED BYPASS TESTS

        [Fact]
        public async Task BypassTest1_REST_BOLA_CannotAccessOtherCustomerTicket()
        {
            AuthenticateAsCustomer();
            var response = await _client.GetAsync("/api/v1/Tickets/9999"); 
            
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task BypassTest2_REST_RBAC_CustomerCannotChangeStatus()
        {
            AuthenticateAsCustomer();
            var payload = new StringContent(JsonSerializer.Serialize(new { status = "Resolved" }), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync("/api/v1/Tickets/1/status", payload); 
            
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); 
        }

        [Fact]
        public async Task BypassTest3_REST_RBAC_CustomerCannotAddInternalNote()
        {
            AuthenticateAsCustomer();
            var payload = new StringContent(JsonSerializer.Serialize(new { note = "Secret Agent Note" }), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/v1/Tickets/1/notes", payload); 
            
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); 
        }

        [Fact]
        public async Task BypassTest4_MCP_RBAC_CustomerCannotUseAnalyticsTool()
        {
            AuthenticateAsCustomer();
            var payload = new StringContent(JsonSerializer.Serialize(new { query = "high priority tickets" }), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/v1/Mcp/get_ticket_analytics", payload); 
            
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound);
        }
    }
}