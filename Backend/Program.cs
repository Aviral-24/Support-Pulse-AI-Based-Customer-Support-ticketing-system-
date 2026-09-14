// using Microsoft.EntityFrameworkCore;
// using Backend.Data;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;
// using Serilog; 
// using Serilog.Formatting.Compact; 
// using System.Text;
// using Backend.Middleware;
// using Microsoft.OpenApi.Models;
// using System.Threading.RateLimiting;
// using Backend.Services;
// using Backend.Services.Async; 
// using Backend.Services.Pdf;
// using Sentry;
// using OpenTelemetry.Metrics;
// using OpenTelemetry.Trace;
// using Backend.Diagnostics;
// using Amazon.S3;
// using Amazon.Runtime;
// using Microsoft.Extensions.Http.Resilience; 
// using StackExchange.Redis; 
// using QuestPDF.Infrastructure; // ✅ DAY 5: QuestPDF namespace zaroori hai
// using Backend.Mcp;

// Log.Logger = new LoggerConfiguration()
//     .Enrich.FromLogContext()
//     .WriteTo.Console(new RenderedCompactJsonFormatter())
//     .CreateBootstrapLogger();

// try
// {
//     Log.Information("Starting Support-Pulse API...");
//     var builder = WebApplication.CreateBuilder(args);

//     // ✅ Setup QuestPDF Community License YAHAN aayega (builder banne ke baad)
//     QuestPDF.Settings.License = LicenseType.Community;

//     builder.Host.UseSerilog((context, services, configuration) => configuration
//         .ReadFrom.Configuration(context.Configuration)
//         .ReadFrom.Services(services)
//         .Enrich.FromLogContext()
//         .WriteTo.Console(new RenderedCompactJsonFormatter()));

//    builder.Services.AddScoped<SupportPulseMcpServer>();

//     // --- SENTRY & OPENTELEMETRY ---
//     builder.WebHost.UseSentry(options =>
//     {
//         options.Dsn = "https://examplePublicKey@o0.ingest.sentry.io/0"; 
//         options.Debug = true;
//         options.TracesSampleRate = 1.0; 
//     });

//     builder.Services.AddOpenTelemetry()
//         .WithTracing(tracerProviderBuilder =>
//         {
//             tracerProviderBuilder
//                 .AddSource("SupportPulse.Backend")
//                 .AddAspNetCoreInstrumentation() 
//                 .AddHttpClientInstrumentation()
//                 .AddConsoleExporter(); 
//         })
//         .WithMetrics(metricsProviderBuilder =>
//         {
//             metricsProviderBuilder
//                 .AddMeter(AiMetrics.Meter.Name) 
//                 .AddAspNetCoreInstrumentation()
//                 .AddHttpClientInstrumentation()
//                 .AddConsoleExporter();
//         });

//     // --- Services Registration Start ---
    
//     // Polly Resilience Pipeline
//     builder.Services.AddHttpClient("OpenAIClient")
//         .AddStandardResilienceHandler(options => {
//             options.Retry.MaxRetryAttempts = 3;
//             options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
//         });

//     // Redis Connection (Lazy Connection)
//     builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
//         ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") + ",abortConnect=false")
//     );
    
//     // AI Services 
//     builder.Services.AddHttpClient<IAIService, AiService>();
//     builder.Services.AddSingleton<ITicketQueue, RedisTicketQueue>(); 
//     builder.Services.AddHostedService<AiEnrichmentWorker>();
    
//     builder.Services.AddHttpContextAccessor();
    
//     // S3 / MinIO Configuration
//     var s3Config = new AmazonS3Config
//     {
//         ServiceURL = "http://s3-minio:9000",
//         ForcePathStyle = true,
//         UseHttp = true 
//     };
//     var awsCredentials = new BasicAWSCredentials("minioadmin", "minioadmin");
//     builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(awsCredentials, s3Config));
//     builder.Services.AddScoped<IStorageService, AwsS3StorageService>();

//     // ✅ Register Day 5 Services (Pdf & Audit)
//     builder.Services.AddScoped<ITicketPdfGenerator, TicketPdfGenerator>();
//     builder.Services.AddScoped<IAuditService, AuditService>();

//     // Controllers & Swagger
//     builder.Services.AddControllers();
//     builder.Services.AddEndpointsApiExplorer();
//     builder.Services.AddSwaggerGen(c =>
//     {
//         c.SwaggerDoc("v1", new OpenApiInfo { Title = "SupportPulse API", Version = "v1" });
//         c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//         {
//             Description = "Enter 'Bearer {token}'",
//             Name = "Authorization",
//             In = ParameterLocation.Header,
//             Type = SecuritySchemeType.ApiKey,
//             Scheme = "Bearer"
//         });
//         c.AddSecurityRequirement(new OpenApiSecurityRequirement {
//         {
//             new OpenApiSecurityScheme {
//                 Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
//             },
//             new string[] { }
//         }});
//     });

//     // Database Connection
//     builder.Services.AddDbContext<ApplicationDbContext>(options =>
//         options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), 
//         o => o.UseVector())); 

//     builder.Services.AddHealthChecks();
    
//     builder.Services.AddCors(options =>
//     {
//         options.AddPolicy("AllowAll", policy =>
//             policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
//     });

//     // JWT Authentication
//     builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//         .AddJwtBearer(options =>
//         {
//             options.TokenValidationParameters = new TokenValidationParameters
//             {
//                 ValidateIssuer = true,
//                 ValidateAudience = true,
//                 ValidateLifetime = true,
//                 ValidateIssuerSigningKey = true,
//                 ValidIssuer = builder.Configuration["Jwt:Issuer"],
//                 ValidAudience = builder.Configuration["Jwt:Audience"],
//                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
//             };
//         });

//     builder.Services.AddRateLimiter(options =>
//     {
//         options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
//             RateLimitPartition.GetFixedWindowLimiter(
//                 partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
//                 factory: _ => new FixedWindowRateLimiterOptions
//                 {
//                     AutoReplenishment = true,
//                     PermitLimit = 100,
//                     Window = TimeSpan.FromMinutes(1)
//                 }));
//     });

//    // builder.Services.AddHostedService<AiEnrichmentWorker>();
    
//     var app = builder.Build();

//     // --- HTTP Request Pipeline Start ---
//     if (app.Environment.IsDevelopment())
//     {
//         app.UseSwagger();
//         app.UseSwaggerUI();
//     }

//     app.UseSerilogRequestLogging(); 
//     app.UseHttpsRedirection();
//     app.UseStaticFiles(); 
//     app.UseMiddleware<ExceptionMiddleware>();
//     app.UseRateLimiter();
//     app.UseCors("AllowAll");
    
//     app.UseAuthentication();
//     app.UseAuthorization();
//     app.UseSentryTracing();
//     app.MapControllers();
//     app.MapHealthChecks("/health");

//     // Database Migration on startup
//     using (var scope = app.Services.CreateScope())
//     {
//         var services = scope.ServiceProvider;
//         var context = services.GetRequiredService<ApplicationDbContext>();
        
//         int maxRetries = 5;
//         for (int i = 0; i < maxRetries; i++)
//         {
//             try
//             {
//                 context.Database.Migrate();
//                 Log.Information("Database migration applied successfully.");
//                 break;
//             }
//             catch (Exception ex)
//             {
//                 Log.Warning(ex, "Database not ready yet (Attempt {Attempt}/{Max}). Retrying in 5 seconds...", i + 1, maxRetries);
//                 if (i == maxRetries - 1) throw;
//                 System.Threading.Thread.Sleep(5000); 
//             }
//         }
//     }
    
//     app.Run();
// }
// catch (Exception ex)
// {
//     Log.Fatal(ex, "Application terminated unexpectedly");
// }
// finally
// {
//     Log.CloseAndFlush();
// }

// public partial class Program {}

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

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// ✅ FIX 1: Static Logger Hata Diya (Parallel xUnit test crashes fix)
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter()));

builder.Services.AddScoped<SupportPulseMcpServer>();

builder.WebHost.UseSentry(options =>
{
    options.Dsn = "https://examplePublicKey@o0.ingest.sentry.io/0"; 
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

// ✅ FIX 2: Redis, DB, aur JWT ke liye Fallback strings add ki (Null Reference fix in Tests)
var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConn + ",abortConnect=false")
);

builder.Services.AddHttpClient<IAIService, AiService>();
builder.Services.AddSingleton<ITicketQueue, RedisTicketQueue>(); 
builder.Services.AddHostedService<AiEnrichmentWorker>();
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

// ✅ DB Fallback for tests
var dbConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=testdb;Username=postgres;Password=postgres";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dbConn, o => o.UseVector())); 

builder.Services.AddHealthChecks();
builder.Services.AddCors(options => { options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); });

// ✅ JWT Fallback for tests
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
            factory: _ => new FixedWindowRateLimiterOptions { AutoReplenishment = true, PermitLimit = 100, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseSerilogRequestLogging(); 
app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();
app.UseCors("AllowAll");
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

app.Run();

public partial class Program {}