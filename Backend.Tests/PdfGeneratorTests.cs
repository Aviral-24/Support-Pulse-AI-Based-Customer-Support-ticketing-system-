using Backend.Models;
using Backend.Services.Pdf;
using QuestPDF.Infrastructure;
using Xunit;

namespace Backend.Tests;

public class PdfGeneratorTests
{
    public PdfGeneratorTests()
    {
        // License set karna zaroori hai tests run karne ke liye
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Fact]
    public void GenerateTicketSummary_ReturnsValidPdfBytes()
    {
        // Arrange
        var generator = new TicketPdfGenerator();
        var mockTicket = new Ticket
        {
            Id = 101,
            Title = "Test Checkout Bug",
            Description = "Payment failed repeatedly.",
            Status = "Open",
            Category = "Payment",
            AiSummary = "Customer is facing payment failure.",
            AiSentiment = "Frustrated"
        };

        // Act
        var pdfBytes = generator.GenerateTicketSummary(mockTicket);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 0); // Check if file is not empty
        
        // Ensure standard PDF header signature starts with %PDF
        Assert.Equal(37, pdfBytes[0]); // '%'
        Assert.Equal(80, pdfBytes[1]); // 'P'
        Assert.Equal(68, pdfBytes[2]); // 'D'
        Assert.Equal(70, pdfBytes[3]); // 'F'
    }
}
