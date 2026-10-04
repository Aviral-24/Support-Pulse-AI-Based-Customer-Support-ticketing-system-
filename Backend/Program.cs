using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog; 
using Serilog.Formatting.Compact; 
using System.Text;
using Backend.Middleware;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using Backend.Services;
using Backend.Services.Async; 
using Backend.Services.Pdf;
using Backend.Workers; 
using Sentry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Backend.Diagnostics;
using Amazon.S3;
using Amazon.Runtime;
using Microsoft.Extensions.Http.Resilience; 
using StackExchange.Redis; 
using QuestPDF.Infrastructure; 
using Backend.Mcp;
using Npgsql;
using Backend.Services.Email;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter()));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<SupportPulseMcpServer>();

// SENTRY SETUP (Corrected)
builder.WebHost.UseSentry(options =>
{
    // Sentry ka DSN link 
    options.Dsn = "https://5bfdb845b2f4e1daf585ce70ddb6fbdc@o4512176154542080.ingest.de.sentry.io/4512176175186000"; 
    options.Debug = true;
    options.TracesSampleRate = 1.0; 
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder.AddSource("SupportPulse.Backend").AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddConsoleExporter(); 
    })
    .WithMetrics(metricsProviderBuilder =>
    {
        metricsProviderBuilder.AddMeter(AiMetrics.Meter.Name).AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddConsoleExporter();
    });

builder.Services.AddHttpClient("OpenAIClient")
    .AddStandardResilienceHandler(options => {
        options.Retry.MaxRetryAttempts = 3;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    });

var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConn + ",abortConnect=false")
);

builder.Services.AddHttpClient<IAIService, AiService>();
builder.Services.AddSingleton<ITicketQueue, RedisTicketQueue>(); 
builder.Services.AddHostedService<TicketProcessingWorker>();
builder.Services.AddHttpContextAccessor();

var s3Config = new AmazonS3Config { ServiceURL = "http://s3-minio:9000", ForcePathStyle = true, UseHttp = true };
var awsCredentials = new BasicAWSCredentials("minioadmin", "minioadmin");
builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(awsCredentials, s3Config));
builder.Services.AddScoped<IStorageService, AwsS3StorageService>();
builder.Services.AddScoped<ITicketPdfGenerator, TicketPdfGenerator>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SupportPulse API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "Enter 'Bearer {token}'", Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey, Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } }});
});

var dbConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=SupportPulse_db;Username=postgres;Password=Abhi@2080";
var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConn);
dataSourceBuilder.UseVector();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource, o => o.UseVector()));

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", 
                "https://aviral-supportpulse.duckdns.org" 
              ) 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "test_issuer";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "test_audience";
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_fallback_key_for_testing_purposes_12345!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions { AutoReplenishment = true, PermitLimit = 50000, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

// FIX: Yahan se duplicate 'builder' line hata di gayi hai!

app.UseSerilogRequestLogging(); 
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors("AllowReact"); 

app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseSentryTracing();
app.MapControllers();
app.MapHealthChecks("/health");

if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        int maxRetries = 5;
        for (int i = 0; i < maxRetries; i++)
        {
            try {
                context.Database.Migrate();
                break;
            } catch (Exception) {
                if (i == maxRetries - 1) throw;
                System.Threading.Thread.Sleep(5000); 
            }
        }
    }
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.SeedSuperAdmin();
}

app.Run();

public partial class Program {}