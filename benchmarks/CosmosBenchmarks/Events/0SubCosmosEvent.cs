using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;

namespace CosmosBenchmarks;

public class NoSubCosmosEvent : ICosmosEvent
{
    public required string MessageBody { get; set;  }
}
class NoSubCosmosEventMapping : IMessageMapping<NoSubCosmosEvent>
{
    public string QueueName => "test-topic";
}
