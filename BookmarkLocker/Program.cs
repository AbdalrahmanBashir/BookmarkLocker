using BookmarkLocker.Config;
using BookmarkLocker.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<CosmosOptions>(builder.Configuration.GetSection("Cosmos"));
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;
    var clientOptions = new CosmosClientOptions
    {
        ApplicationName = "BookmarkLocker",
        AllowBulkExecution = true,

        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    var client = new CosmosClient(options.AccountEndpoint, options.AccountKey, clientOptions);

    var db = client.CreateDatabaseIfNotExistsAsync(options.DatabaseName).GetAwaiter().GetResult();

    db.Database.CreateContainerIfNotExistsAsync(new ContainerProperties{
        Id = options.ContainerName,
        PartitionKeyPath = options.PartitionKeyPath}, throughput: 400).GetAwaiter().GetResult();
    return client;
});

builder.Services.AddSingleton<IBookmarkService, BookmarkService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
