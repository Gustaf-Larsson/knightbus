using KnightBus.Core;
using KnightBus.Messages;
using KnightBus.Azure.ServiceBus.Messages;

namespace ServiceBusBenchmarks.Events;

public class SixteenSubEventProcessor :
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription1, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription2, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription3, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription4, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription5, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription6, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription7, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription8, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription9, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription10, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription11, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription12, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription13, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription14, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription15, ProcessingSettings>,
    IProcessEvent<SixteenSubEvent, SixteenSubSubscription16, ProcessingSettings>
{
    public Task ProcessAsync(SixteenSubEvent message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class SixteenSubEvent : IServiceBusEvent
{
    public required string MessageBody { get; set;  }
}
public class SixteenSubEventMapping : IMessageMapping<SixteenSubEvent>
{
    public string QueueName => "sixteen_sub";
}
public class SixteenSubSubscription1: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_1";
}

public class SixteenSubSubscription2: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_2";
}

public class SixteenSubSubscription3: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_3";
}

public class SixteenSubSubscription4: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_4";
}

public class SixteenSubSubscription5: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_5";
}

public class SixteenSubSubscription6: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_6";
}

public class SixteenSubSubscription7: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_7";
}

public class SixteenSubSubscription8: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_8";
}

public class SixteenSubSubscription9: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_9";
}

public class SixteenSubSubscription10: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_10";
}

public class SixteenSubSubscription11: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_11";
}

public class SixteenSubSubscription12: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_12";
}

public class SixteenSubSubscription13: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_13";
}

public class SixteenSubSubscription14: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_14";
}

public class SixteenSubSubscription15: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_15";
}

public class SixteenSubSubscription16: IEventSubscription<SixteenSubEvent>
{
    public string Name => "subscription_16";
}
