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
    .Build();

await host.RunAsync();