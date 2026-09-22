using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Backend.Controllers; // Apke controller ka namespace
using Xunit;

namespace Units_Tests;

public class ProfileControllerTests
{
    [Fact]
    public void GetMyProfile_WithValidToken_ReturnsCorrectUserIdAndRole()
    {
        // 1. ARRANGE: Ek fake JWT Token payload (Claims) create karna
        var expectedUserId = "101";
        var expectedRole = "Customer";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId),
            new Claim(ClaimTypes.Role, expectedRole)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Fake HttpContext banana jisme hamara fake user login hai
        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        var controller = new ProfileController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        // 2. ACT: API method ko direct call karna
        var result = controller.GetMyProfile() as OkObjectResult;

        // 3. ASSERT: Verify karna ki API ne fail toh nahi kiya
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // Verify karna ki anonymous object me correct details aayi hain
        var responseValue = result.Value;
        Assert.NotNull(responseValue);

        var actualUserId = responseValue.GetType().GetProperty("userId")?.GetValue(responseValue, null);
        var actualRole = responseValue.GetType().GetProperty("role")?.GetValue(responseValue, null);

        // Agar ye pass hua, matlab controller token ko perfectly samajh raha hai
        Assert.Equal(expectedUserId, actualUserId);
        Assert.Equal(expectedRole, actualRole);
    }
}