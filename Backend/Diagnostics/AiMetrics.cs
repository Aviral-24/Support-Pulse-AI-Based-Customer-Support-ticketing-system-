using System.Diagnostics.Metrics;

namespace Backend.Diagnostics;

public static class AiMetrics
{
    // 1. Define a custom Meter for our application
    public static readonly Meter Meter = new("SupportPulse.AiMetrics", "1.0");

    // 2. Track AI Processing Time (Latency)
    public static readonly Histogram<double> AiProcessingLatency = Meter.CreateHistogram<double>(
        name: "ai_processing_latency_ms",
        unit: "ms",
        description: "Time taken to process AI enrichment for a ticket");

    // 3. Track AI Failures/Errors
    public static readonly Counter<int> AiErrorCount = Meter.CreateCounter<int>(
        name: "ai_error_count",
        description: "Number of failed AI processing attempts");

    // Helper property to track queue depth globally
    public static int CurrentQueueDepth { get; set; } = 0;

    // 4. Track Queue Depth
    public static readonly ObservableGauge<int> QueueDepth = Meter.CreateObservableGauge<int>(
        name: "ticket_queue_depth",
        observeValue: () => CurrentQueueDepth,
        description: "Current number of tickets waiting for AI enrichment");
}