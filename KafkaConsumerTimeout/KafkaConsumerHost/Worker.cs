using Confluent.Kafka;

namespace KafkaConsumerHost;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ClientConfig _kafkaClientConfig;

    public Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplicationLifetime, ClientConfig kafkaClientConfig)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _kafkaClientConfig = kafkaClientConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var timeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
        var consumerConfig = new ConsumerConfig(_kafkaClientConfig)
        {
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            SessionTimeoutMs = timeout,
            MaxPollIntervalMs = timeout,
            GroupId = "dotnet-worker-service"
        };
        using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetLogHandler((_, logMessage) =>
                _logger.LogInformation("Kafka Log Message Received: {facility} - {message}", logMessage.Facility,
                    logMessage.Message))
            .Build();
            
        consumer.Subscribe("quickstart");
        _logger.LogInformation("Subscribed to topic");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(stoppingToken);
                _logger.LogInformation("Message received: {message}", consumeResult.Message.Value);
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                _logger.LogInformation("About to get a Kafka exception because poll interval exceeded");
                consumer.Commit(consumeResult);
            }
        }
        catch (OperationCanceledException)
        {
            // Ctrl+C
        }
        catch (KafkaException kex)
        {
            _logger.LogError(kex, "Kafka Exception encountered.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception encountered in consumer.");
        }
        finally
        {
            consumer.Close();
            _hostApplicationLifetime.StopApplication();
        }
    }
}
