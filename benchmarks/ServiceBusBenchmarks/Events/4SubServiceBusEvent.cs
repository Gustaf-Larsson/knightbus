using KnightBus.Core;
using KnightBus.Messages;
using KnightBus.Azure.ServiceBus.Messages;

namespace ServiceBusBenchmarks.Events;

public class FourSubEvent : IServiceBusEvent
{
    public required string MessageBody { get; set;  }
}
public class FourSubEventMapping : IMessageMapping<FourSubEvent>
{
    public string QueueName => "four_sub";
}
public class FourSubSubscription1: IEventSubscription<FourSubEvent>
{
    public string Name => "subscription_1";
}

public class FourSubSubscription2: IEventSubscription<FourSubEvent>
{
    public string Name => "subscription_2";
}

public class FourSubSubscription3: IEventSubscription<FourSubEvent>
{
    public string Name => "subscription_3";
}

public class FourSubSubscription4: IEventSubscription<FourSubEvent>
{
    public string Name => "subscription_4";
}

public class FourSubEventProcessor :
    IProcessEvent<FourSubEvent, FourSubSubscription1, ProcessingSettings>,
    IProcessEvent<FourSubEvent, FourSubSubscription2, ProcessingSettings>,
    IProcessEvent<FourSubEvent, FourSubSubscription3, ProcessingSettings>,
    IProcessEvent<FourSubEvent, FourSubSubscription4, ProcessingSettings>
{
    public Task ProcessAsync(FourSubEvent message, CancellationToken cancellationToken)
    {
        ProcessedMessages.Increment(message.MessageBody);
        return Task.CompletedTask;
    }
}
