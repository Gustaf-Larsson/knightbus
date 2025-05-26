using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;

namespace CosmosBenchmarks.Events;

class TwoSubEventProcessor :
    IProcessEvent<TwoSubCosmosEvent, TwoSubSubscription1, CosmosProcessingSettings>,
    IProcessEvent<TwoSubCosmosEvent, TwoSubSubscription2, CosmosProcessingSettings>
{
    public Task ProcessAsync(TwoSubCosmosEvent message, CancellationToken cancellationToken)
    {
        ProcessedMessages.Increment(message.MessageBody);
        return Task.CompletedTask;
    }
}

public class TwoSubCosmosEvent : ICosmosEvent
{
    public required string MessageBody { get; set;  }
}
class TwoSubCosmosEventMapping : IMessageMapping<TwoSubCosmosEvent>
{
    public string QueueName => "TwoSub";
}
class TwoSubSubscription1: IEventSubscription<TwoSubCosmosEvent>
{
    public string Name => "subscription_1";
}

class TwoSubSubscription2: IEventSubscription<TwoSubCosmosEvent>
{
    public string Name => "subscription_2";
}
