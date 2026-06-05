using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class DynamoDbServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder().Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonDynamoDBClient CreateClient()
    {
        return new AmazonDynamoDBClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonDynamoDBConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsTable()
    {
        using var dynamoDb = CreateClient();
        const string tableName = "test-table";

        await dynamoDb.CreateTableAsync(new CreateTableRequest
        {
            TableName = tableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new() { AttributeName = "Id", AttributeType = ScalarAttributeType.S },
            },
            KeySchema = new List<KeySchemaElement>
            {
                new() { AttributeName = "Id", KeyType = KeyType.HASH },
            },
        });

        var tables = await dynamoDb.ListTablesAsync(new ListTablesRequest());

        Assert.Contains(tableName, tables.TableNames);
    }
}
