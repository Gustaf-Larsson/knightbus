using KnightBus.Core;
using KnightBus.Messages;
using KnightBus.Azure.ServiceBus.Messages;

namespace ServiceBusBenchmarks.Events;

public class EightSubEvent : IServiceBusEvent
{
    public required string MessageBody { get; set;  }
}
public class EightSubEventMapping : IMessageMapping<EightSubEvent>
{
    public string QueueName => "eight_sub";
}
public class EightSubSubscription1: IEventSubscription<EightSubEvent>
{
    public string Name => "subscription_1";
}

public class EightSubSubscription2: IEventSubscription<EightSubEvent>
{
    public string Name => "subscription_2";
}

public class EightSubSubscription3: IEventSubscription<EightSubEvent>
{
    public string Name => "subscription_3";
}

public class EightSubSubscription4: IEventSubscription<EightSubEvent>
{
    public string Name => "subscription_4";
}

public class EightSubSubscription5: IEventSubscription<EightSubEvent>
{
    public string Name => "subscription_5";
}

public class EightSubSubscription6: IEventSubscription<EightSubEvent>
{
    public string Name => "subscription_6";
}

public class EightSubSubscription7: IEventSubscription<EightSubEvent>
{
    public string Name => "subscription_7";
}

public class EightSubSubscription8: IEventSubscription<EightSubEvent>
{
    public string Name => "subscription_8";
}

public class EightSubEventProcessor :
    IProcessEvent<EightSubEvent, EightSubSubscription1, ProcessingSettings>,
    IProcessEvent<EightSubEvent, EightSubSubscription2, ProcessingSettings>,
    IProcessEvent<EightSubEvent, EightSubSubscription3, ProcessingSettings>,
    IProcessEvent<EightSubEvent, EightSubSubscription4, ProcessingSettings>,
    IProcessEvent<EightSubEvent, EightSubSubscription5, ProcessingSettings>,
    IProcessEvent<EightSubEvent, EightSubSubscription6, ProcessingSettings>,
    IProcessEvent<EightSubEvent, EightSubSubscription7, ProcessingSettings>,
    IProcessEvent<EightSubEvent, EightSubSubscription8, ProcessingSettings>
{
    public Task ProcessAsync(EightSubEvent message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

