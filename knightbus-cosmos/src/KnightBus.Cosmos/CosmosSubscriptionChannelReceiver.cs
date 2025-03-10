﻿using System.Data;
using System.Net;
using KnightBus.Core;
using KnightBus.Cosmos.Messages;
using KnightBus.Messages;
using Microsoft.Azure.Cosmos;

namespace KnightBus.Cosmos;

public class CosmosSubscriptionChannelReceiver<T> : IChannelReceiver where T : class, ICosmosEvent
{
    private IProcessingSettings _processorSettings;
    private IMessageSerializer _serializer;
    private IEventSubscription _subscription;
    private IHostConfiguration _configuration;
    private IMessageProcessor _processor;
    private ICosmosConfiguration _cosmosConfiguration;
    
    public CosmosSubscriptionChannelReceiver(
        IProcessingSettings processorSettings,
        IMessageSerializer serializer,
        IEventSubscription subscription,
        IHostConfiguration config,
        IMessageProcessor processor,
        ICosmosConfiguration cosmosConfiguration
    )
    {
        _processorSettings = processorSettings;
        _serializer = serializer;
        _subscription = subscription;
        _configuration = config;
        _processor = processor;
        _cosmosConfiguration = cosmosConfiguration;

    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //Create cosmos client
        CosmosClient client = new CosmosClient(_cosmosConfiguration.ConnectionString);
        
        //Get database, create if it does not exist
        DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync(_cosmosConfiguration.Database, 400, null, cancellationToken);
        CheckResponse(databaseResponse, _cosmosConfiguration.Database);
        
        Database database = databaseResponse.Database;
        //Get container, create if it does not exist
        Container items = await CreateContainerIfNotExistsOrIncompatibleAsync(client, database, _cosmosConfiguration.Container, "/topic", (int)_cosmosConfiguration.DefaultTimeToLive.TotalSeconds);
        
        Container leaseContainer = await CreateContainerIfNotExistsOrIncompatibleAsync(client, database, "lease", "/id", (int)_cosmosConfiguration.DefaultTimeToLive.TotalSeconds);

         ChangeFeedProcessor changeFeedProcessor = items
            .GetChangeFeedProcessorBuilder<T>(
                processorName: "changeFeed",
                onChangesDelegate: HandleChangesAsync)
            .WithInstanceName("consoleHost")
            .WithLeaseContainer(leaseContainer)
            .WithPollInterval(_cosmosConfiguration.PollingDelay)
            .Build();

         try
         {
             await changeFeedProcessor.StartAsync();
             Console.WriteLine("Change feed processor started.");
         }
         catch
         {
             Console.WriteLine("Failed to start change feed processor");
         }
    }
    
    // TODO: Should call the processAsync in program so that the process behaviour can be defined there
    //Change Feed Handler
    private async Task HandleChangesAsync(
        IReadOnlyCollection<T> changes, 
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Changes: {changes.Count}");
        foreach (var change in changes)
        {
            // Print the message_data received
            Console.WriteLine($"Message {change.id} received with data: {change.messageBody}");

            var stateHandler = new CosmosMessageStateHandler<T>();
            //await _processor.ProcessAsync(stateHandler, cancellationToken);
        }
    }

    private static void CheckResponse(object response, string id)
    {
        switch (response)
        {
            case DatabaseResponse databaseResponse:
                CheckHttpResponse(databaseResponse.StatusCode , "database", id);
                break;
            case ContainerResponse containerResponse:
                CheckHttpResponse(containerResponse.StatusCode, "container", id);
                break;
            default:
                throw new ArgumentException(
                    "Invalid response type, only databaseResponse & containerResponse are supported");
        }
    }
    private static void CheckHttpResponse(HttpStatusCode statusCode, string type, string id)
    {
        switch (statusCode)
        {
            case HttpStatusCode.Created:
                Console.WriteLine($"{type}: {id} created");
                break;
            case HttpStatusCode.OK:
                Console.WriteLine($"{type}: {id} already exists");
                break;
            default:
                throw new Exception($"Unexpected http response code when creating {type} {id} : {statusCode}");
        }
    }
    

    //Create Container if it does not exist, old containers with incompatible settings to the new one crash server
    private static async Task<Container> CreateContainerIfNotExistsOrIncompatibleAsync(CosmosClient client, Database db, string containerId, string partitionKey, int defaultTTL)
    {
        //Get container
        
        //Get metadata to check compatibility with new container
        
        //Create a new container
        var response = await db.CreateContainerIfNotExistsAsync(
            new ContainerProperties(containerId, partitionKey) {DefaultTimeToLive = defaultTTL});
        CheckResponse(response, containerId);

        return response.Container;
    }
    
    public IProcessingSettings Settings { get; set; }
}
