using System.Collections.Concurrent;
using KnightBus.Core;
using KnightBus.Core.DependencyInjection;
using KnightBus.Cosmos;
using KnightBus.Cosmos.Messages;
using KnightBus.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using CosmosBenchmarks.Events;

namespace CosmosBenchmarks;

public class CosmosLatencyBenchmarks
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(300);
    private CosmosBus? _publisher;
    private IHost? _knightBusHost;
    private const string DatabaseName = "PubSub";
    const string LeaseContainer = "Leases";
    public static async Task Main(String[] args)
    {
        if (args == null || args.Length < 2)
        {
            throw new Exception("2 arguments required");
        }

        CosmosLatencyBenchmarks cosmosBenchmarks = new CosmosLatencyBenchmarks();

        int numMessages = int.Parse(args[0]);
        int numSubs = int.Parse(args[1]);

        switch (numSubs)
        {
            case 1:
                await cosmosBenchmarks.TimeFromSendToProcessed<OneSubCosmosEvent, OneSubEventProcessor>(
                    i => new OneSubCosmosEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    1);
                break;
            case 2:
                await cosmosBenchmarks.TimeFromSendToProcessed<TwoSubCosmosEvent, TwoSubEventProcessor>(
                    i => new TwoSubCosmosEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    2);
                break;
            case 4:
                await cosmosBenchmarks.TimeFromSendToProcessed<FourSubCosmosEvent, FourSubEventProcessor>(
                    i => new FourSubCosmosEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    4);
                break;
            case 8:
                await cosmosBenchmarks.TimeFromSendToProcessed<EightSubCosmosEvent, EightSubEventProcessor>(
                    i => new EightSubCosmosEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    8);
                break;
            case 16:
                await cosmosBenchmarks.TimeFromSendToProcessed<SixteenSubCosmosEvent, SixteenSubEventProcessor>(
                    i => new SixteenSubCosmosEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    16);
                break;
            default:
                throw new ArgumentException("Second argument must be either 1,2,4,8 or 16");
        }
    }
    
    //Benchmark the elapsed time to send messages
    private async Task TimeFromSendToProcessed<TMessage, TProcessor>(
        Func<int, TMessage> messageFactory,
        Func<TMessage, string> getMessageBody,
        int numMessages,
        int subscribers)
        where TProcessor : class
        where TMessage : ICosmosEvent
    {
        await Setup<TProcessor>();
        
        //Send some commands
        TMessage[] messages = new TMessage[numMessages];
        for (int i = 0; i < numMessages; i++)
        {
            TMessage message = messageFactory(i);
            messages[i] = message;
        }
        
        var startTime = DateTime.UtcNow;
        await _publisher!.PublishAsync(messages, CancellationToken.None);
        TimeSpan elapsedTime = DateTime.UtcNow.Subtract(startTime);
        Console.Error.WriteLine($"time: {elapsedTime}");
        await TearDown();
    }
    
    //Setup
    private async Task Setup<TEventProcessor>() where TEventProcessor : class
    {
        //Connection string should be saved as environment variable named "CosmosString"
        var connectionString = Environment.GetEnvironmentVariable("CosmosString");

        _knightBusHost = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            })
            .ConfigureServices(services =>
            {
                services.UseCosmos(configuration =>
                    {
                        configuration.ConnectionString = connectionString;
                        configuration.Database = DatabaseName;
                        configuration.LeaseContainer = LeaseContainer;
                        configuration.PollingDelay = TimeSpan.FromMilliseconds(500);
                        configuration.DefaultTimeToLive = TimeSpan.FromSeconds(180);
                    })
                    .RegisterProcessor<TEventProcessor>()
                    .UseTransport<CosmosTransport>();
            })
            .UseKnightBus()
            .Build();

        //Start the KnightBus Host
        await _knightBusHost.StartAsync();
        await Task.Delay(TimeSpan.FromSeconds(1));

        _publisher = _knightBusHost.Services.CreateScope().ServiceProvider.GetRequiredService<CosmosBus>();
    }
    
    //Delete cosmos database and close connection
    public async Task TearDown()
    {
        await _publisher!.RemoveDatabase();
        _knightBusHost?.Dispose();
        _publisher!.Dispose();
    }
}
