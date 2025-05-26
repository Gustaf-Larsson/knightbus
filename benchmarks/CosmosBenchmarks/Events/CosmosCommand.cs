using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;

namespace CosmosBenchmarks.Events;


//Commands
class CosmosCommand : ICosmosCommand
{
    public required string MessageBody { get; set; }
}

class SampleCosmosMessageMapping : IMessageMapping<CosmosCommand>
{
    public string QueueName => "Command";
}

class CosmosCommandProcessor :
    IProcessCommand<CosmosCommand, CosmosProcessingSettings>
{
    public Task ProcessAsync(CosmosCommand message, CancellationToken cancellationToken)
    {
        ProcessedMessages.Increment(message.MessageBody);

        return Task.CompletedTask;
    }
}
