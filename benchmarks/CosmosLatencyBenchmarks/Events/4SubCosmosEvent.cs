using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;

namespace CosmosBenchmarks.Events;

class FourSubEventProcessor :
    IProcessEvent<FourSubCosmosEvent, FourSubSubscription1, CosmosProcessingSettings>,
    IProcessEvent<FourSubCosmosEvent, FourSubSubscription2, CosmosProcessingSettings>,
    IProcessEvent<FourSubCosmosEvent, FourSubSubscription3, CosmosProcessingSettings>,
    IProcessEvent<FourSubCosmosEvent, FourSubSubscription4, CosmosProcessingSettings>
{
    public Task ProcessAsync(FourSubCosmosEvent message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class FourSubCosmosEvent : ICosmosEvent
{
    public required string MessageBody { get; set;  }
}
class FourSubCosmosEventMapping : IMessageMapping<FourSubCosmosEvent>
{
    public string QueueName => "FourSub";
}
class FourSubSubscription1: IEventSubscription<FourSubCosmosEvent>
{
    public string Name => "subscription_1";
}

class FourSubSubscription2: IEventSubscription<FourSubCosmosEvent>
{
    public string Name => "subscription_2";
}

class FourSubSubscription3: IEventSubscription<FourSubCosmosEvent>
{
    public string Name => "subscription_3";
}

class FourSubSubscription4: IEventSubscription<FourSubCosmosEvent>
{
    public string Name => "subscription_4";
}
