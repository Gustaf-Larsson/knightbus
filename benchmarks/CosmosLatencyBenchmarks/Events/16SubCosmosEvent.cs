using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;

namespace CosmosBenchmarks.Events;

class SixteenSubEventProcessor :
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription1, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription2, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription3, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription4, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription5, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription6, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription7, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription8, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription9, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription10, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription11, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription12, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription13, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription14, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription15, CosmosProcessingSettings>,
    IProcessEvent<SixteenSubCosmosEvent, SixteenSubSubscription16, CosmosProcessingSettings>
{
    public Task ProcessAsync(SixteenSubCosmosEvent message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class SixteenSubCosmosEvent : ICosmosEvent
{
    public required string MessageBody { get; set;  }
}
class SixteenSubCosmosEventMapping : IMessageMapping<SixteenSubCosmosEvent>
{
    public string QueueName => "SixteenSub";
}
class SixteenSubSubscription1: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_1";
}

class SixteenSubSubscription2: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_2";
}

class SixteenSubSubscription3: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_3";
}

class SixteenSubSubscription4: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_4";
}

class SixteenSubSubscription5: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_5";
}

class SixteenSubSubscription6: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_6";
}

class SixteenSubSubscription7: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_7";
}

class SixteenSubSubscription8: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_8";
}

class SixteenSubSubscription9: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_9";
}

class SixteenSubSubscription10: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_10";
}

class SixteenSubSubscription11: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_11";
}

class SixteenSubSubscription12: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_12";
}

class SixteenSubSubscription13: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_13";
}

class SixteenSubSubscription14: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_14";
}

class SixteenSubSubscription15: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_15";
}

class SixteenSubSubscription16: IEventSubscription<SixteenSubCosmosEvent>
{
    public string Name => "subscription_16";
}
