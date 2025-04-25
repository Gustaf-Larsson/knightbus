using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;

namespace CosmosBenchmarks;

class CosmosProcessingSettings : IProcessingSettings
{
    public int MaxConcurrentCalls => 10; //Currently not used
    public int PrefetchCount => 50; //Currently not used
    public TimeSpan MessageLockTimeout => TimeSpan.FromMinutes(5); //Currently not used
    public int DeadLetterDeliveryLimit => 2;
}
