using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;

namespace CosmosBenchmarks.Events;

class EightSubEventProcessor :
    IProcessEvent<EightSubCosmosEvent, EightSubSubscription1, CosmosProcessingSettings>,
    IProcessEvent<EightSubCosmosEvent, EightSubSubscription2, CosmosProcessingSettings>,
    IProcessEvent<EightSubCosmosEvent, EightSubSubscription3, CosmosProcessingSettings>,
    IProcessEvent<EightSubCosmosEvent, EightSubSubscription4, CosmosProcessingSettings>,
    IProcessEvent<EightSubCosmosEvent, EightSubSubscription5, CosmosProcessingSettings>,
    IProcessEvent<EightSubCosmosEvent, EightSubSubscription6, CosmosProcessingSettings>,
    IProcessEvent<EightSubCosmosEvent, EightSubSubscription7, CosmosProcessingSettings>,
    IProcessEvent<EightSubCosmosEvent, EightSubSubscription8, CosmosProcessingSettings>
{
    public Task ProcessAsync(EightSubCosmosEvent message, CancellationToken cancellationToken)
    {
        ProcessedMessages.Increment(message.MessageBody);
        return Task.CompletedTask;
    }
}

public class EightSubCosmosEvent : ICosmosEvent
{
    public required string MessageBody { get; set;  }
}
class EightSubCosmosEventMapping : IMessageMapping<EightSubCosmosEvent>
{
    public string QueueName => "EightSub";
}
class EightSubSubscription1: IEventSubscription<EightSubCosmosEvent>
{
    public string Name => "subscription_1";
}

class EightSubSubscription2: IEventSubscription<EightSubCosmosEvent>
{
    public string Name => "subscription_2";
}

class EightSubSubscription3: IEventSubscription<EightSubCosmosEvent>
{
    public string Name => "subscription_3";
}

class EightSubSubscription4: IEventSubscription<EightSubCosmosEvent>
{
    public string Name => "subscription_4";
}

class EightSubSubscription5: IEventSubscription<EightSubCosmosEvent>
{
    public string Name => "subscription_5";
}

class EightSubSubscription6: IEventSubscription<EightSubCosmosEvent>
{
    public string Name => "subscription_6";
}

class EightSubSubscription7: IEventSubscription<EightSubCosmosEvent>
{
    public string Name => "subscription_7";
}

class EightSubSubscription8: IEventSubscription<EightSubCosmosEvent>
{
    public string Name => "subscription_8";
}


