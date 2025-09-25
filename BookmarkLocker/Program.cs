using BookmarkLocker.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<CosmosOptions>(options =>
{
    options.AccountEndpoint = builder.Configuration["Cosmos:AccountEndpoint"] ?? string.Empty;
    options.DatabaseName = builder.Configuration["Cosmos:DatabaseName"] ?? string.Empty;
    options.ContainerName = builder.Configuration["Cosmos:ContainerName"] ?? string.Empty;
    options.PartitionKeyPath = builder.Configuration["Cosmos:PartitionKeyPath"] ?? "/userId";
});

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
