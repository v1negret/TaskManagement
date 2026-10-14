namespace TaskManagement.API.Services;

public class RabbitMqHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqHostedService> _logger;
    public static RabbitMqPublisher? Publisher { get; set; }
    
    public RabbitMqHostedService(
        IServiceProvider sp,
        ILogger<RabbitMqHostedService> logger)
    {
        _serviceProvider = sp;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken ctx)
    {
        try
        {
            Publisher = await RabbitMqPublisher.CreateAsync(
                _serviceProvider.GetRequiredService<IConfiguration>(),
                _serviceProvider.GetRequiredService<ILogger<RabbitMqPublisher>>(),
                ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ unavailable at startup, will retry on first publish");
        }
    }

    public async Task StopAsync(CancellationToken ctx)
    {
        if (Publisher is null) return;
        
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ctx);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await Publisher.DisposeAsync().AsTask().WaitAsync(timeout.Token);
            _logger.LogInformation("RabbitMQ publisher closed gracefully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("RabbitMQ publisher did not close within 5 seconds, forcing");
        }
    }
}