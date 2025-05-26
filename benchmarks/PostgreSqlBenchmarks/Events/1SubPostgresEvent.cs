using KnightBus.Core;
using KnightBus.Messages;
using KnightBus.PostgreSql.Messages;

namespace PostgresBenchmarks.Events;


public class OneSubEvent : IPostgresEvent
{
    public required string MessageBody { get; set; }
}
public class OneSubEventMapping : IMessageMapping<OneSubEvent>
{
    public string QueueName => "one_sub";
}

public class OneSubEventSubscription1: IEventSubscription<OneSubEvent>
{
    public string Name => "subscription_1";
}


public class OneSubEventProcessor :
    IProcessEvent<OneSubEvent, OneSubEventSubscription1, ProcessingSettings>
{

    public Task ProcessAsync(OneSubEvent message, CancellationToken cancellationToken)
    {
        ProcessedMessages.Increment(message.MessageBody);
        return Task.CompletedTask;
    }
}
