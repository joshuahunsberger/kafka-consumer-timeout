using System.Globalization;
using Confluent.Kafka;
using KafkaConsumerHost;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(new ClientConfig
        {
            BootstrapServers = "localhost:9092"
        });
        services.AddHostedService<Worker>();
    })
    .ConfigureLogging(logBuilder =>
    {
        logBuilder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = $"[{DateTimeFormatInfo.CurrentInfo.SortableDateTimePattern}] ";
        });
    })
    .Build();

await host.RunAsync();
