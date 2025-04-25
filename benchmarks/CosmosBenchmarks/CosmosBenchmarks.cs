using System.Collections.Concurrent;
using KnightBus.Core;
using KnightBus.Core.DependencyInjection;
using KnightBus.Cosmos;
using KnightBus.Cosmos.Messages;
using KnightBus.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace CosmosBenchmarks;

public class CosmosBenchmarks
{
    private static readonly int[] MessageCounts = [10000, 100000, 500000];
    private const int BenchmarkIterations = 10;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(300);
    private CosmosBus? _publisher;
    private IHost? _knightBusHost;
    private const string DatabaseName = "PubSub";
    const string LeaseContainer = "Leases";
    private static readonly string Path = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "CosmosBenchmarks",
        $"Real_{DateTime.Now:MM-dd-HH-mm}.txt"
    );
    public static async Task Main(String[] args)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!); //Create document

        await File.WriteAllTextAsync(Path, string.Empty); //Create empty output file

        foreach  (int numMessages in MessageCounts)
        {
            await File.AppendAllTextAsync(Path, $"{Environment.NewLine}....... {numMessages} ........ {Environment.NewLine}");
            
            //Aspect - Number of subscribers on topic (small msg)
            //Write 1 sub events
            await Benchmark<OneSubCosmosEvent, OneSubEventProcessor>(i => new OneSubCosmosEvent() {MessageBody = $"event {i}"},
                msg => msg.MessageBody, numMessages, 1);
            
            //Write 2 sub events
            await Benchmark<TwoSubCosmosEvent, TwoSubEventProcessor>(i => new TwoSubCosmosEvent() { MessageBody = $"event {i}" },
                msg => msg.MessageBody,numMessages, 2
            );
            
            //Write 4 sub events
            await Benchmark<FourSubCosmosEvent, FourSubEventProcessor>(i => new FourSubCosmosEvent() { MessageBody = $"event {i}" },
                msg => msg.MessageBody,numMessages, 4
            );
            
            //Write 8 sub events
            await Benchmark<EightSubCosmosEvent, EightSubEventProcessor>(i => new EightSubCosmosEvent() { MessageBody = $"event {i}" },
                msg => msg.MessageBody, numMessages ,8
            );
            
            //Write 16 sub events
            await Benchmark<SixteenSubCosmosEvent, SixteenSubEventProcessor>(i => new SixteenSubCosmosEvent() { MessageBody = $"event {i}" },
                msg => msg.MessageBody, numMessages,16
            );
        }
    }
    
    // Benchmark multiple iterations and write the results to file
    private static async Task Benchmark<TMessage, TCommandProcessor>(
        Func<int, TMessage> messageFactory,
        Func<TMessage, string> getMessageBody,
        int numMessages,
        int subscribers = 1)
        where TCommandProcessor : class
        where TMessage : ICosmosEvent
    {
        await using var writer = new StreamWriter(Path, append: true);
        await writer.WriteLineAsync($"---------- {typeof(TMessage)} ------------");
        for (int i = 0; i < BenchmarkIterations; i++)
        {
            var sendCommands = new CosmosBenchmarks();
            
            var result =
                await sendCommands.TimeFromSendToProcessed<TMessage, TCommandProcessor>(messageFactory, getMessageBody, numMessages, subscribers);
            await writer.WriteLineAsync(
                $"Time: {result.Item1.TotalSeconds} | missed: {result.Item2} | duplicates: {result.Item3}");
        }
    }
    
    //Benchmark the elapsed time from sending messages to all messages being processed
    private async Task<(TimeSpan, int, int)> TimeFromSendToProcessed<TMessage, TProcessor>(
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
        bool withinTime = true;
        
        int missed = 0;
        int lastMissed = 0;
        int sameMisses = 0;
        while ((missed = (ProcessedMessages.MissedAndDuplicateProcessed(messageContents, subscribers).Item1)) > 0 &&
               (withinTime = (DateTime.UtcNow.Subtract(startTime)) < Timeout))
        {
            if (missed > 0 && missed == lastMissed)
            {
                sameMisses++;
            }
            else
            {
                lastMissed = missed;
                sameMisses = 0;
            }

            //If no new messages have been processed for the past 1s, assume those messages are lost
            if (sameMisses >= 100)
                break;
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        TimeSpan elapsedTime = withinTime ? DateTime.UtcNow.Subtract(startTime) : TimeSpan.Zero;
        
        (int notProcessed, int duplicate) = ProcessedMessages.MissedAndDuplicateProcessed(messageContents, subscribers);
        await TearDown();
        return (elapsedTime, notProcessed, duplicate);
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

    public static (int,int) MissedAndDuplicateProcessed(IEnumerable<string> messageStrings, int deliveriesPerMessage = 1)
    {
        int notProcessed = 0;
        int duplicateProcessed = 0;
        foreach (var id in messageStrings)
        {
            if (!dict.TryGetValue(id, out var value) || value < deliveriesPerMessage)
            {
                notProcessed++;
            }
            else if(value > deliveriesPerMessage)
            {
                duplicateProcessed++;
            }
        }
        return (notProcessed, duplicateProcessed);
    }
}

