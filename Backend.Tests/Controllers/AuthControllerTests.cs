// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Moq;
// using Xunit;
// using Backend.Controllers;
// using Backend.Data;
// using Backend.Models;
// using Backend.DTOs;
// using BCrypt.Net;

// namespace Backend.Tests.Controllers
// {
//     // 🔥 NINJA FIX: In-Memory DB ke liye Custom Context banaya
//     public class TestDbContext : ApplicationDbContext
//     {
//         public TestDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);
//             // EF Core In-Memory DB ko bata rahe hain ki Pgvector ko completely ignore kare!
//             modelBuilder.Ignore<Pgvector.Vector>(); 
//         }
//     }

//     public class AuthControllerTests
//     {
//         private readonly Mock<IConfiguration> _mockConfiguration;

//         public AuthControllerTests()
//         {
//             // 🟢 ARRANGE
//             _mockConfiguration = new Mock<IConfiguration>();
//             _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("super_secret_fallback_key_for_testing_purposes_12345!");
//             _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("test_issuer");
//             _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("test_audience");
//         }

//         private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
//         {
//             return new DbContextOptionsBuilder<ApplicationDbContext>()
//                 .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
//                 .Options;
//         }

//         [Fact]
//         public async Task Register_NewUser_ReturnsOkAndSavesToDb()
//         {
//             var options = CreateNewContextOptions();
//             // 🔥 Yahan ApplicationDbContext ki jagah TestDbContext use kiya hai
//             using var context = new TestDbContext(options);
//             var controller = new AuthController(context, _mockConfiguration.Object);

//             var request = new RegisterDto { Name = "Rahul", Email = "rahul@customer.com", Password = "Password123" };

//             var result = await controller.Register(request);

//             var okResult = Assert.IsType<OkObjectResult>(result);
//             var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
//             Assert.NotNull(savedUser);
//             Assert.Equal("Customer", savedUser.Role);
//             Assert.True(BCrypt.Net.BCrypt.Verify(request.Password, savedUser.PasswordHash));
//         }

//         [Fact]
//         public async Task Register_AgentEmail_AssignsAgentRole()
//         {
//             var options = CreateNewContextOptions();
//             using var context = new TestDbContext(options);
//             var controller = new AuthController(context, _mockConfiguration.Object);

//             var request = new RegisterDto { Name = "Agent Amit", Email = "amit.agent@support.com", Password = "Pass" };

//             await controller.Register(request);

//             var savedUser = await context.Users.FirstAsync();
//             Assert.Equal("Agent", savedUser.Role);
//         }

//         [Fact]
//         public async Task Register_ExistingUser_ReturnsBadRequest()
//         {
//             var options = CreateNewContextOptions();
//             using var context = new TestDbContext(options);
            
//             context.Users.Add(new User { Name = "Old User", Email = "test@test.com", PasswordHash = "hash", Role = "Customer" });
//             await context.SaveChangesAsync();

//             var controller = new AuthController(context, _mockConfiguration.Object);
//             var request = new RegisterDto { Name = "New User", Email = "test@test.com", Password = "123" };

//             var result = await controller.Register(request);

//             var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//             Assert.Equal("User already exists.", badRequestResult.Value);
//         }

//         [Fact]
//         public async Task Login_ValidCredentials_ReturnsOkWithToken()
//         {
//             var options = CreateNewContextOptions();
//             using var context = new TestDbContext(options);
            
//             string rawPassword = "MySecretPassword!";
//             context.Users.Add(new User 
//             { 
//                 Name = "Valid User", 
//                 Email = "valid@test.com", 
//                 PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword),
//                 Role = "Admin" 
//             });
//             await context.SaveChangesAsync();

//             var controller = new AuthController(context, _mockConfiguration.Object);
//             var request = new LoginDto { Email = "valid@test.com", Password = rawPassword };

//             var result = await controller.Login(request);

//             var okResult = Assert.IsType<OkObjectResult>(result);
//             var responseValue = okResult.Value;
            
//             Assert.NotNull(responseValue); // 🔥 FIX: Ye CS8602 warning hatayega

//             var roleProp = responseValue.GetType().GetProperty("role")?.GetValue(responseValue, null);
//             var tokenProp = responseValue.GetType().GetProperty("token")?.GetValue(responseValue, null);
            
//             Assert.Equal("Admin", roleProp);
//             Assert.NotNull(tokenProp);
//         }

//         [Fact]
//         public async Task Login_InvalidPassword_ReturnsUnauthorized()
//         {
//             var options = CreateNewContextOptions();
//             using var context = new TestDbContext(options);
            
//             context.Users.Add(new User 
//             { 
//                 Name = "Hacker", 
//                 Email = "hacker@test.com", 
//                 PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"), 
//                 Role = "Customer" 
//             });
//             await context.SaveChangesAsync();

//             var controller = new AuthController(context, _mockConfiguration.Object);
//             var request = new LoginDto { Email = "hacker@test.com", Password = "WrongPassword123!" };

//             var result = await controller.Login(request);

//             var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
//             Assert.Equal("Invalid credentials.", unauthorizedResult.Value);
//         }
//     }
// }
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using BCrypt.Net;

namespace Backend.Tests.Controllers
{
    // 🔥 NINJA FIX: In-Memory DB ke liye Custom Context banaya
    public class TestDbContext : ApplicationDbContext
    {
        public TestDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // EF Core In-Memory DB ko bata rahe hain ki Pgvector ko completely ignore kare!
            modelBuilder.Ignore<Pgvector.Vector>(); 
        }
    }

    public class AuthControllerTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;

        public AuthControllerTests()
        {
            // 🟢 ARRANGE
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("super_secret_fallback_key_for_testing_purposes_12345!");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("test_issuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("test_audience");
        }

        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;
        }

        [Fact]
        public async Task Register_NewUser_ReturnsOkAndSavesToDb()
        {
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);
            var controller = new AuthController(context, _mockConfiguration.Object);

            var request = new RegisterDto { Name = "Rahul", Email = "rahul@customer.com", Password = "Password123", Role = "Customer" };

            var result = await controller.Register(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            Assert.NotNull(savedUser);
            Assert.Equal("Customer", savedUser.Role);
            Assert.True(BCrypt.Net.BCrypt.Verify(request.Password, savedUser.PasswordHash));
        }

        [Fact]
        public async Task Register_AdminRole_AssignsAdminRole()
        {
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);
            var controller = new AuthController(context, _mockConfiguration.Object);

            // 🔥 NINJA FIX: Ab email me agent dhoondhne ki bajay direct DTO me Role bhej rahe hain (Frontend Dropdown logic)
            var request = new RegisterDto { Name = "Admin User", Email = "admin@support.com", Password = "Pass", Role = "Admin" };

            await controller.Register(request);

            var savedUser = await context.Users.FirstAsync();
            Assert.Equal("Admin", savedUser.Role);
        }

        [Fact]
        public async Task Register_ExistingUser_ReturnsBadRequest()
        {
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);
            
            context.Users.Add(new User { Name = "Old User", Email = "test@test.com", PasswordHash = "hash", Role = "Customer" });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, _mockConfiguration.Object);
            var request = new RegisterDto { Name = "New User", Email = "test@test.com", Password = "123", Role = "Customer" };

            var result = await controller.Register(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User already exists.", badRequestResult.Value);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);
            
            string rawPassword = "MySecretPassword!";
            context.Users.Add(new User 
            { 
                Name = "Valid User", 
                Email = "valid@test.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword),
                Role = "Admin" 
            });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, _mockConfiguration.Object);
            var request = new LoginDto { Email = "valid@test.com", Password = rawPassword };

            var result = await controller.Login(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // 🔥 NINJA FIX: Anonymous Types ko read karne ke liye JSON serialization best approach hai
            var json = JsonSerializer.Serialize(okResult.Value);
            var jsonDoc = JsonDocument.Parse(json);
            
            var roleProp = jsonDoc.RootElement.GetProperty("role").GetString();
            var tokenProp = jsonDoc.RootElement.GetProperty("token").GetString();
            
            Assert.Equal("Admin", roleProp);
            Assert.NotNull(tokenProp);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var options = CreateNewContextOptions();
            using var context = new TestDbContext(options);
            
            context.Users.Add(new User 
            { 
                Name = "Hacker", 
                Email = "hacker@test.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"), 
                Role = "Customer" 
            });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, _mockConfiguration.Object);
            var request = new LoginDto { Email = "hacker@test.com", Password = "WrongPassword123!" };

            var result = await controller.Login(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value); // 🔥 FIX: CS8602 Warning hata di
            
            // 🔥 FIX: Direct string comparison object/value type ke sath
            Assert.Equal("Invalid credentials.", unauthorizedResult.Value.ToString());
        }
    }
}