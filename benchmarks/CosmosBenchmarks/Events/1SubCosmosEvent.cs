using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;

namespace CosmosBenchmarks;

class OneSubEventProcessor :
    IProcessEvent<OneSubCosmosEvent, SampleSubscription, CosmosProcessingSettings>
{
    public Task ProcessAsync(OneSubCosmosEvent message, CancellationToken cancellationToken)
    {
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
