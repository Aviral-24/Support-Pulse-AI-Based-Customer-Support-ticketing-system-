using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Backend.Controllers;

namespace Backend.Tests.Controllers
{
    public class ProfileControllerTests
    {
        // Helper: Fake User Token banane ke liye taaki test crash na ho
        private ProfileController CreateControllerWithFakeUser(string userId, string role)
        {
            var controller = new ProfileController();
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public void GetMyProfile_ReturnsOk_WithUserIdAndRole()
        {
            // 🟢 ARRANGE
            // Hum ek fake "Customer" banayenge jiska ID 42 hai
            var controller = CreateControllerWithFakeUser("42", "Customer");

            // 🟢 ACT
            var result = controller.GetMyProfile();

            // 🟢 ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseValue = okResult.Value;
            
            Assert.NotNull(responseValue);
            
            // C# Reflection se anonymous object ki property check kar rahe hain
            var userIdProp = responseValue.GetType().GetProperty("userId")?.GetValue(responseValue, null);
            var roleProp = responseValue.GetType().GetProperty("role")?.GetValue(responseValue, null);

            Assert.Equal("42", userIdProp);
            Assert.Equal("Customer", roleProp);
        }

        [Fact]
        public void GetAgentData_ReturnsOk_WithMessage()
        {
            // 🟢 ARRANGE
            // Hum ek fake "Agent" banayenge jiska ID 99 hai
            var controller = CreateControllerWithFakeUser("99", "Agent");

            // 🟢 ACT
            var result = controller.GetAgentData();

            // 🟢 ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseValue = okResult.Value;
            
            Assert.NotNull(responseValue);
            
            var msgProp = responseValue.GetType().GetProperty("message")?.GetValue(responseValue, null);
            Assert.Equal("This is secured agent data. Customers cannot see this!", msgProp);
        }
    }
}