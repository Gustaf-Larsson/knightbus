using KnightBus.Core;
using KnightBus.Messages;
using KnightBus.PostgreSql.Messages;

namespace PostgresBenchmarks.Events;


public class TwoSubEvent : IPostgresEvent
{
    public required string MessageBody { get; set;  }
}
public class TwoSubEventMapping : IMessageMapping<TwoSubEvent>
{
    public string QueueName => "two_sub";
}

public class TwoSubEventSubscription1: IEventSubscription<TwoSubEvent>
{
    public string Name => "subscription_1";
}

public class TwoSubEventSubscription2: IEventSubscription<TwoSubEvent>
{
    public string Name => "subscription_2";
}


public class TwoSubEventProcessor :
    IProcessEvent<TwoSubEvent, TwoSubEventSubscription1, ProcessingSettings>,
    IProcessEvent<TwoSubEvent, TwoSubEventSubscription2, ProcessingSettings>
{
    public Task ProcessAsync(TwoSubEvent message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
