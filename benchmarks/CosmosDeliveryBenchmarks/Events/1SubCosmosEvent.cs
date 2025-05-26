using CosmosDeliveryBenchmarks;
using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;

namespace CosmosBenchmarks.Events;

class OneSubEventProcessor :
    IProcessEvent<OneSubCosmosEvent, SampleSubscription, CosmosProcessingSettings>
{
    public Task ProcessAsync(OneSubCosmosEvent message, CancellationToken cancellationToken)
    {
        if (Random.Shared.Next(0, 5) == 0)
        {
            throw new Exception("Simulated Error");
        }
        ProcessedMessages.Increment(message.MessageBody);
        return Task.CompletedTask;
    }
}


public class OneSubCosmosEvent : ICosmosEvent
{
    public required string MessageBody { get; set;  }
}
class OneSubCosmosEventMapping : IMessageMapping<OneSubCosmosEvent>
{
    public string QueueName => "test-topic";
}
class SampleSubscription: IEventSubscription<OneSubCosmosEvent>
{
    public string Name => "sample_subscription";
}
