using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus.Administration;
using KnightBus.Core;
using KnightBus.Core.DependencyInjection;
using KnightBus.Host;
using KnightBus.Messages;
using KnightBus.Azure.ServiceBus;
using KnightBus.Azure.ServiceBus.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.ResourceManager.ServiceBus;

using ServiceBusBenchmarks.Events;

namespace ServiceBusBenchmarks;

public class ServiceBusLatencyBenchmarks
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(600);
    private ServiceBus _publisher;
    private IHost? _knightBusHost;
    public static async Task Main(String[] args)
    {
        if (args == null || args.Length < 2)
        {
            throw new Exception("2 arguments required");
        }

        ServiceBusLatencyBenchmarks serviceBusLatencyBenchmarks = new ServiceBusLatencyBenchmarks();

        int numMessages = int.Parse(args[0]);
        int numSubs = int.Parse(args[1]);

        switch (numSubs)
        {
            case 1:
                await serviceBusLatencyBenchmarks.TimeFromSendToProcessed<OneSubEvent, OneSubEventProcessor>(
                    i => new OneSubEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    1);
                break;
            case 2:
                await serviceBusLatencyBenchmarks.TimeFromSendToProcessed<TwoSubEvent, TwoSubEventProcessor>(
                    i => new TwoSubEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    2);
                break;
            case 4:
                await serviceBusLatencyBenchmarks.TimeFromSendToProcessed<FourSubEvent, FourSubEventProcessor>(
                    i => new FourSubEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    4);
                break;
            case 8:
                await serviceBusLatencyBenchmarks.TimeFromSendToProcessed<EightSubEvent, EightSubEventProcessor>(
                    i => new EightSubEvent() { MessageBody = $"event {i}" },
                    msg => msg.MessageBody,
                    numMessages,
                    8);
                break;
            case 16:
                await serviceBusLatencyBenchmarks.TimeFromSendToProcessed<SixteenSubEvent, SixteenSubEventProcessor>(
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
        where TMessage : IServiceBusEvent
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
        await _publisher.PublishEventsAsync(messages, CancellationToken.None);
        TimeSpan elapsedTime = DateTime.UtcNow.Subtract(startTime);
        Console.Error.WriteLine($"time: {elapsedTime}");
        await TearDown<TMessage>();
    }
    
    //Setup
    private async Task Setup<TEventProcessor>() where TEventProcessor : class
    {
        //Connection string should be saved as environment variable named "ServiceBusString"
        var connectionString = Environment.GetEnvironmentVariable("ServiceBusString");


        _knightBusHost = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            })
            .ConfigureServices(services =>
            {
                services.UseServiceBus(configuration =>
                    {
                        configuration.ConnectionString = connectionString;
                    })
                    .RegisterProcessor<TEventProcessor>()
                    .UseTransport<ServiceBusTransport>();
            })
            .UseKnightBus(c => c.ShutdownGracePeriod = TimeSpan.FromSeconds(2))
            .Build();

        //Start the KnightBus Host
        await _knightBusHost.StartAsync();
        await Task.Delay(TimeSpan.FromSeconds(5));

        _publisher = (ServiceBus)_knightBusHost.Services.CreateScope().ServiceProvider.GetRequiredService<IServiceBus>();
    }
    
    //Delete ServiceBus database and close connection
    public async Task TearDown<TMessage>() where TMessage : IServiceBusEvent
    {
        var managementClient = new ServiceBusAdministrationClient(Environment.GetEnvironmentVariable("ServiceBusString"));
        try
        {
            await managementClient.DeleteTopicAsync(AutoMessageMapper.GetQueueName<TMessage>());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Deletion failed: {ex.Message}");
        }
    }
}




