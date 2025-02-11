﻿using KnightBus.Core;

namespace KnightBus.Cosmos;

public class CosmosTransport : ITransport
{
    public CosmosTransport(string connectionString) : this(new CosmosConfiguration(connectionString))
    { }

    public CosmosTransport(ICosmosConfiguration configuration)
    {
        TransportChannelFactories = new ITransportChannelFactory[]
        {
            new CosmosQueueChannelFactory(configuration),
            //new CosmosTopicChannelFactory(configuration)
        };
    }

    public ITransportChannelFactory[] TransportChannelFactories { get; }

    public ITransport ConfigureChannels(ITransportConfiguration configuration)
    {
        foreach (var channelFactory in TransportChannelFactories)
        {
            channelFactory.Configuration = configuration;
        }

        return this;
    }
}
