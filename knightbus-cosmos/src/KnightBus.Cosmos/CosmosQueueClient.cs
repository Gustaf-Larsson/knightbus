﻿using System.Net;
using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;
using Microsoft.Azure.Cosmos;

namespace KnightBus.Cosmos;

public class CosmosQueueClient<T> where T : class, IMessage
{
    private readonly ICosmosConfiguration _cosmosConfiguration;
    public CosmosClient Client { get; private set; }
    public Database Database { get; private set; }
    public Container TopicQueue { get; private set; }
    public Container RetryQueue { get; private set; }
    public Container Lease { get; private set; }
    private Container DeadLetterQueue { get; set; }
    
    public IEventSubscription? _subscription { get; private set; }
    
    public CosmosQueueClient(ICosmosConfiguration cosmosConfiguration, IEventSubscription? subscription)
    {
        _cosmosConfiguration = cosmosConfiguration;
        _subscription = subscription;
    }

    // subscription client

    public async Task StartAsync(CosmosClient cosmosClient, CancellationToken cancellationToken)
    {
        //Create cosmos client
        Client = cosmosClient;
        
        //Get database, create if it does not exist
        DatabaseResponse databaseResponse = await Client.CreateDatabaseIfNotExistsAsync(_cosmosConfiguration.Database, 1000, null, cancellationToken);
        CheckHttpResponse(databaseResponse.StatusCode, _cosmosConfiguration.Database);
        Database = databaseResponse.Database;
        
        Lease = await CreateContainerAsync(_cosmosConfiguration.LeaseContainer, cancellationToken); 
        TopicQueue = await CreateContainerAsync(AutoMessageMapper.GetQueueName<T>(), cancellationToken);
        
        if (typeof(ICosmosEvent).IsAssignableFrom(typeof(T)))
        {
            RetryQueue = await CreateContainerAsync(AutoMessageMapper.GetQueueName<T>() + "-" + _subscription.Name + "-Retry", cancellationToken);
            DeadLetterQueue = await CreateContainerAsync(AutoMessageMapper.GetQueueName<T>() + "-" + _subscription.Name + "-DL", cancellationToken, -1 ); //Deadlettered items should never expire
        }
        else if (typeof(ICosmosCommand).IsAssignableFrom(typeof(T)))
        {
            DeadLetterQueue = await CreateContainerAsync(AutoMessageMapper.GetQueueName<T>() + "-DL", cancellationToken, -1 ); //Deadlettered items should never expire
        }
    }
    
    private async Task<Container> CreateContainerAsync(string id, CancellationToken cancellationToken, int? TTL = null)
    {
        TTL ??= (int)_cosmosConfiguration.DefaultTimeToLive.TotalSeconds; //Get configured TTL if not specified
        const string partitionKeyPath = "/id";
        
        //Get container
        //Update it if it exists and does not match expectations
        try
        {
            Container container = Database.GetContainer(id);
            ContainerProperties properties = (await container.ReadContainerAsync(cancellationToken: cancellationToken)).Resource;

            if (properties.PartitionKeyPath == partitionKeyPath && properties.DefaultTimeToLive == TTL)
            {
                return container;
            }

            properties.PartitionKeyPath = partitionKeyPath;
            properties.DefaultTimeToLive = TTL;
                
            await container.ReplaceContainerAsync(properties, cancellationToken: cancellationToken); //Update container properties, does not loose data

            return container;
        }
        catch
        {
            //Create container if it does not exist
            var response = await Database.CreateContainerIfNotExistsAsync(
                new ContainerProperties(id, "/id") {DefaultTimeToLive = TTL}, cancellationToken: cancellationToken);
            CheckHttpResponse(response.StatusCode, id);
            return response.Container;
        }
    }
    
    private static void CheckHttpResponse(HttpStatusCode statusCode, string id)
    {
        if(statusCode != HttpStatusCode.OK && statusCode != HttpStatusCode.Created) 
            throw new Exception($"Unexpected http response when creating {id} : {statusCode}");
    }
    
    public async Task CompleteAsync(InternalCosmosMessage<T> message)
    {
        await Task.CompletedTask;
    }

    public async Task AbandonByErrorAsync(InternalCosmosMessage<T> message)
    {
        message.DeliveryCount++;
        //Upsert item into retryQueue
        ItemResponse<InternalCosmosMessage<T>> response = await RetryQueue.UpsertItemAsync<InternalCosmosMessage<T>>(
            message, new PartitionKey(message.id));
    }

    public async Task DeadLetterAsync(InternalCosmosMessage<T> message)
    {
        await DeadLetterQueue.CreateItemAsync(message, new PartitionKey(message.id));
    }

    public async Task AddItemAsync(InternalCosmosMessage<T> message, Container container)
    {
        await container.CreateItemAsync(message, new PartitionKey(message.id));
    }
    
    public async Task RemoveItemAsync(InternalCosmosMessage<T> message, Container container)
    {
        await container.DeleteItemAsync<InternalCosmosMessage<T>>(message.id, new PartitionKey(message.id));
    }
}
