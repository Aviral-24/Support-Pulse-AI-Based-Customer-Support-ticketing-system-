// using System;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Moq;
// using Xunit;
// using Backend.Controllers;
// using Backend.Mcp;
// using static Backend.Controllers.McpController;

// namespace Backend.Tests.Controllers
// {
//     public class McpControllerTests
//     {
//         // Concrete class ko mock kar rahe hain
//         private readonly Mock<SupportPulseMcpServer> _mockMcpServer;
//         private readonly McpController _controller;

//         public McpControllerTests()
//         {
//             // 🟢 ARRANGE: Setup Mock Server
//             // Agar class mock karte waqt error aaye, toh SupportPulseMcpServer me in methods ko 'virtual' banana padega
//             _mockMcpServer = new Mock<SupportPulseMcpServer>();
//             _controller = new McpController(_mockMcpServer.Object);
//         }

//         [Fact]
//         public async Task ExecuteTool_GetTicketAnalytics_ReturnsOkWithResult()
//         {
//             // 🟢 ARRANGE
//             // JsonDocument ka use karke ek fake AI JSON object banaya
//             string jsonParams = "{\"category\":\"Hardware\",\"status\":\"Open\"}";
//             var parameters = JsonDocument.Parse(jsonParams).RootElement;
            
//             var request = new McpRequest 
//             { 
//                 ToolName = "get_ticket_analytics", 
//                 Parameters = parameters 
//             };

//             string fakeAiResult = "{\"total\": 10, \"resolved\": 5}";
            
//             // Mocking the behavior of our MCP server
//             _mockMcpServer.Setup(m => m.GetTicketAnalyticsAsync("Hardware", "Open", null))
//                           .ReturnsAsync(fakeAiResult);

//             // 🟢 ACT
//             var result = await _controller.ExecuteTool(request);

//             // 🟢 ASSERT
//             var okResult = Assert.IsType<OkObjectResult>(result);
//             var responseValue = okResult.Value;
            
//             Assert.NotNull(responseValue);
            
//             var successProp = responseValue.GetType().GetProperty("success")?.GetValue(responseValue, null);
//             Assert.Equal(true, successProp);
//         }

//         [Fact]
//         public async Task ExecuteTool_GeneratePdfSummary_ValidId_ReturnsBase64()
//         {
//             // 🟢 ARRANGE
//             string jsonParams = "{\"ticketId\": 123}";
//             var parameters = JsonDocument.Parse(jsonParams).RootElement;
            
//             var request = new McpRequest 
//             { 
//                 ToolName = "generate_pdf_summary", 
//                 Parameters = parameters 
//             };

//             byte[] fakePdfBytes = new byte[] { 1, 2, 3, 4, 5 }; // Fake PDF data
//             _mockMcpServer.Setup(m => m.GeneratePdfSummaryToolAsync(123))
//                           .ReturnsAsync(fakePdfBytes);

//             // 🟢 ACT
//             var result = await _controller.ExecuteTool(request);

//             // 🟢 ASSERT
//             var okResult = Assert.IsType<OkObjectResult>(result);
//             var responseValue = okResult.Value;
            
//             Assert.NotNull(responseValue);
            
//             var base64Prop = responseValue.GetType().GetProperty("base64Data")?.GetValue(responseValue, null);
//             var expectedBase64 = Convert.ToBase64String(fakePdfBytes);
            
//             Assert.Equal(expectedBase64, base64Prop);
//         }

//         [Fact]
//         public async Task ExecuteTool_GeneratePdfSummary_MissingTicketId_ReturnsBadRequest()
//         {
//             // 🟢 ARRANGE
//             // AI ne galti se ticketId bhejna bhool gaya
//             string jsonParams = "{\"category\":\"Hardware\"}"; 
//             var parameters = JsonDocument.Parse(jsonParams).RootElement;
            
//             var request = new McpRequest 
//             { 
//                 ToolName = "generate_pdf_summary", 
//                 Parameters = parameters 
//             };

//             // 🟢 ACT
//             var result = await _controller.ExecuteTool(request);

//             // 🟢 ASSERT
//             var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//             var responseValue = badRequestResult.Value;
            
//             Assert.NotNull(responseValue);
//             var successProp = responseValue.GetType().GetProperty("success")?.GetValue(responseValue, null);
//             var messageProp = responseValue.GetType().GetProperty("message")?.GetValue(responseValue, null);
            
//             Assert.Equal(false, successProp);
//             Assert.Equal("Missing or invalid 'ticketId' parameter.", messageProp);
//         }

//         [Fact]
//         public async Task ExecuteTool_UnknownTool_ReturnsBadRequest()
//         {
//             // 🟢 ARRANGE
//             var request = new McpRequest 
//             { 
//                 ToolName = "hack_the_system", // Ek aisi tool jo exist nahi karti
//                 Parameters = JsonDocument.Parse("{}").RootElement 
//             };

//             // 🟢 ACT
//             var result = await _controller.ExecuteTool(request);

//             // 🟢 ASSERT
//             var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//             var responseValue = badRequestResult.Value;
            
//             Assert.NotNull(responseValue);
//             var successProp = responseValue.GetType().GetProperty("success")?.GetValue(responseValue, null);
//             Assert.Equal(false, successProp);
//         }

//         [Fact]
//         public async Task ExecuteTool_ThrowsException_Returns500InternalServerError()
//         {
//             // 🟢 ARRANGE
//             string jsonParams = "{\"category\":\"Hardware\"}";
//             var parameters = JsonDocument.Parse(jsonParams).RootElement;
//             var request = new McpRequest 
//             { 
//                 ToolName = "get_ticket_analytics", 
//                 Parameters = parameters 
//             };

//             // Force MCP Server to throw an exception
//             _mockMcpServer.Setup(m => m.GetTicketAnalyticsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//                           .ThrowsAsync(new Exception("Database exploded!"));

//             // 🟢 ACT
//             var result = await _controller.ExecuteTool(request);

//             // 🟢 ASSERT
//             var statusCodeResult = Assert.IsType<ObjectResult>(result);
//             Assert.Equal(500, statusCodeResult.StatusCode);
            
//             var responseValue = statusCodeResult.Value;
//             Assert.NotNull(responseValue);
            
//             var messageProp = responseValue.GetType().GetProperty("message")?.GetValue(responseValue, null);
//             Assert.Equal("Database exploded!", messageProp);
//         }
//     }
// }