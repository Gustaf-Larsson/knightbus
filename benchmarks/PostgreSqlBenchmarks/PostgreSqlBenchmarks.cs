using System.Collections.Concurrent;
using KnightBus.Core;
using KnightBus.Core.DependencyInjection;
using KnightBus.Host;
using KnightBus.Messages;
using KnightBus.PostgreSql;
using KnightBus.PostgreSql.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using PostgresBenchmarks.Events;

namespace PostgresBenchmarks;

public class PostgreSqlBenchmarks
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(300);
    private PostgresBus _publisher;
    private IHost? _knightBusHost;
    public static async Task Main(String[] args)
    {
        if (args == null || args.Length < 2)
        {
            throw new Exception("2 arguments required");
        }

        PostgreSqlBenchmarks postgreSqlBenchmarks = new PostgreSqlBenchmarks();

        int numMessages = int.Parse(args[0]);
        int numSubs = int.Parse(args[1]);

        switch (numSubs)
        {
            case 1:
                await postgreSqlBenchmarks.TimeFromSendToProcessed<OneSubEvent, OneSubEventProcessor>(
                    i => new OneSubEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    1);
                break;
            case 2:
                await postgreSqlBenchmarks.TimeFromSendToProcessed<TwoSubEvent, TwoSubEventProcessor>(
                    i => new TwoSubEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    2);
                break;
            case 4:
                await postgreSqlBenchmarks.TimeFromSendToProcessed<FourSubEvent, FourSubEventProcessor>(
                    i => new FourSubEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    4);
                break;
            case 8:
                await postgreSqlBenchmarks.TimeFromSendToProcessed<EightSubEvent, EightSubEventProcessor>(
                    i => new EightSubEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    8);
                break;
            case 16:
                await postgreSqlBenchmarks.TimeFromSendToProcessed<SixteenSubEvent, SixteenSubEventProcessor>(
                    i => new SixteenSubEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    16);
                break;
            default:
                throw new ArgumentException("Second argument must be either 1,2,4,8 or 16");
        }
    }
    
    //Benchmark the elapsed time from sending messages to all messages being processed
    private async Task TimeFromSendToProcessed<TMessage, TProcessor>(
        Func<int, TMessage> messageFactory,
        Func<TMessage, string> getMessageBody,
        int numMessages,
        int subscribers)
        where TProcessor : class
        where TMessage : IPostgresEvent
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
        await _publisher.PublishAsync(messages, CancellationToken.None);

        bool withinTime = true;
        int missed;
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

            //If no new messages have been processed for the at least 5s, assume those messages are lost
            if (sameMisses >= 500)
                break;
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        TimeSpan elapsedTime = withinTime ? DateTime.UtcNow.Subtract(startTime) : TimeSpan.Zero;
        
        (int notProcessed, int duplicate) = ProcessedMessages.MissedAndDuplicateProcessed(messageContents, subscribers);
        await TearDown<TMessage>();
        
        Console.Error.WriteLine($"Time: {elapsedTime} | missed: {notProcessed} | duplicates: {duplicate}");
    }
    
    //Setup
    private async Task Setup<TEventProcessor>() where TEventProcessor : class
    {
        //Connection string should be saved as environment variable named "PostgreSqlString"
        var connectionString = Environment.GetEnvironmentVariable("PostgreString")+"Include Error Detail=true";

        _knightBusHost = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            })
            .ConfigureServices(services =>
            {
                services.UsePostgres(configuration =>
                    {
                        configuration.ConnectionString = connectionString;
                        configuration.PollingDelay = TimeSpan.FromMilliseconds(500);
                    })
                    .RegisterProcessor<TEventProcessor>()
                    .UseTransport<PostgresTransport>();

                services.AddSingleton<ProcessedMessages>();
            })
            .UseKnightBus(c => c.ShutdownGracePeriod = TimeSpan.FromSeconds(2))
            .Build();

        //Start the KnightBus Host
        await _knightBusHost.StartAsync();
        await Task.Delay(TimeSpan.FromSeconds(5));

        _publisher = (PostgresBus)_knightBusHost.Services.CreateScope().ServiceProvider.GetRequiredService<IPostgresBus>();
    }
    
    //Delete PostgreSql database and close connection
    public async Task TearDown<TMessage>() where TMessage : IPostgresEvent
    {
        ProcessedMessages.dict = new ConcurrentDictionary<string, int>();
        _knightBusHost?.Dispose();
        await using var conn = new NpgsqlConnection(Environment.GetEnvironmentVariable("PostgreString"));
        await conn.OpenAsync();

        var dropSchema = $"DROP Schema IF EXISTS knightbus CASCADE";

        await using var cmd = new NpgsqlCommand(dropSchema, conn);
        try
        {
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Schema dropped successfully - {dropSchema}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error dropping Schema: {ex.Message}");
        }

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




