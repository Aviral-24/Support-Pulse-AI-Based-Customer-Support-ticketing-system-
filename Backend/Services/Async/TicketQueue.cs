using StackExchange.Redis;
using Backend.Diagnostics;
using Microsoft.Extensions.Logging; // Logger ke liye

namespace Backend.Services.Async;

public interface ITicketQueue
{
    ValueTask EnqueueTicketAsync(int ticketId);
    ValueTask<int> DequeueTicketAsync(CancellationToken cancellationToken);
}

public class RedisTicketQueue : ITicketQueue
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisTicketQueue> _logger;
    private const string QueueKey = "ticket_queue";

    // ✅ FIX 1: IConnectionMultiplexer aur ILogger ko Dependency Injection se liya
    public RedisTicketQueue(IConnectionMultiplexer muxer, ILogger<RedisTicketQueue> logger)
    {
        _db = muxer.GetDatabase();
        _logger = logger;
    }

    public async ValueTask EnqueueTicketAsync(int ticketId)
    {
        // 1. Redis List ke Left me ticket daalo
        await _db.ListLeftPushAsync(QueueKey, ticketId);
        
        // ✅ FIX 2: Local counter ki jagah exact Redis Queue ki length nikali (Distributed safe)
        var length = await _db.ListLengthAsync(QueueKey);
        AiMetrics.CurrentQueueDepth = (int)length;

        // ✅ FIX 3: Console.WriteLine ki jagah proper ILogger use kiya (Day 6 Observability)
        _logger.LogInformation("[REDIS QUEUE] Ticket {TicketId} Enqueued. Current Queue Length: {Length}", ticketId, length);
    }

    public async ValueTask<int> DequeueTicketAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Redis List ke Right se ticket nikalo
                var ticketId = await _db.ListRightPopAsync(QueueKey);
                
                if (ticketId.HasValue)
                {
                    // Metrics update karo ticket nikalne ke baad
                    var length = await _db.ListLengthAsync(QueueKey);
                    AiMetrics.CurrentQueueDepth = (int)length;
                    
                    _logger.LogInformation("[REDIS QUEUE] Ticket {TicketId} Dequeued. Remaining Length: {Length}", (int)ticketId, length);
                    
                    return (int)ticketId;
                }
            }
            catch (Exception ex)
            {
                // Agar Redis server down ho jaye toh worker crash nahi hona chahiye
                _logger.LogError(ex, "[REDIS QUEUE] Error reading from queue.");
            }

            // Agar queue khali hai ya koi error aayi, toh 1 second wait karke dobara check karo
            await Task.Delay(1000, cancellationToken);
        }
        
        return 0; // Agar service rukne wali ho
    }
}