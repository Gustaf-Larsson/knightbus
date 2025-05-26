using System.Collections.Concurrent;
using CosmosBenchmarks.Events;
using KnightBus.Core;
using KnightBus.Core.DependencyInjection;
using KnightBus.Cosmos;
using KnightBus.Cosmos.Messages;
using KnightBus.Host;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CosmosDeliveryBenchmarks;

public class CosmosDeliveryBenchmarks
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(300);
    private CosmosBus? _publisher;
    private IHost? _knightBusHost;
    private const string DatabaseName = "PubSub";
    const string LeaseContainer = "Leases";
    public static async Task Main(String[] args)
    {
        if (args == null || args.Length < 1)
        {
            throw new Exception("1 arguments required");
        }

        CosmosDeliveryBenchmarks cosmosDeliveryBenchmarks = new CosmosDeliveryBenchmarks();

        int numMessages = int.Parse(args[0]);
        
        await cosmosDeliveryBenchmarks.TimeFromSendToProcessed<OneSubCosmosEvent, OneSubEventProcessor>(
                i => new OneSubCosmosEvent() { MessageBody = $"event {i}" },
                msg => msg.MessageBody,
                numMessages,
                1);
    }
    
    //Benchmark the elapsed time from sending messages to all messages being processed
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
        string[] messageContents = new string[numMessages];
        for (int i = 0; i < numMessages; i++)
        {
            TMessage message = messageFactory(i);
            messages[i] = message;
            messageContents[i] = getMessageBody(message);
        }
        
        var startTime = DateTime.UtcNow;
        
        await _publisher!.PublishAsync(messages, CancellationToken.None);

        await Task.Delay(TimeSpan.FromSeconds(10));
        
        (int processed, int deadLettered, int missed) = await ProcessedMessages.ProcessedAndDeadLettered(messageContents, subscribers);
        await TearDown();
        
        Console.Error.WriteLine($"Processed: {processed}, DeadLettered: {deadLettered}, Missed: {missed}");
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

                services.AddSingleton<ProcessedMessages>();
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
        ProcessedMessages.dict = new ConcurrentDictionary<string, int>();
        
        await _publisher!.RemoveDatabase();
        _knightBusHost?.Dispose();
        _publisher!.Dispose();
    }
}

class ProcessedMessages()
{
    //Uses message strings as id - therefore all message strings need to be unique
    public static ConcurrentDictionary<string, int> dict { get; set; } = new ConcurrentDictionary<string, int>();

    public static void Increment(string key)
    {
        dict.AddOrUpdate(key, 1, (_, val) => val + 1);
    }

    public static async Task<List<T>> GetDeadLetterEvents<T>()
    {
        var connectionString = Environment.GetEnvironmentVariable("CosmosString");
        CosmosClient cosmosClient = new CosmosClient(connectionString);
        Container container = cosmosClient.GetContainer("PubSub", "test-topic : DL");
        var query = container.GetItemQueryIterator<T>("SELECT * FROM c");
        List<T> results = new List<T>();

        while (query.HasMoreResults)
        {
            FeedResponse<T> response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }
    
    public static async Task<(int,int, int)> ProcessedAndDeadLettered(IEnumerable<string> messageStrings, int deliveriesPerMessage = 1)
    {
        var deadLetterEvents = await GetDeadLetterEvents<InternalCosmosMessage<OneSubCosmosEvent>>();
        var deadLetterBodies = new List<String>();
        foreach (var internalEvent in deadLetterEvents)
        {
            deadLetterBodies.Add(internalEvent.Message.MessageBody);
        }
        
        int processed = 0;
        int deadLettered = 0;
        int missed = 0;
        foreach (var id in messageStrings)
        {
            if (!dict.TryGetValue(id, out var value))
            {
                if (deadLetterBodies.Contains(id))
                {
                    deadLettered++;
                }
                else
                {
                    missed++;
                }
            }
            else
            {
                processed++;
            }
        }
        return (processed, deadLettered, missed);
    }
}

