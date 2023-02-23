using Elasticsearch.Net;
using Index.Domain.Consumers;
using Index.Infrastructure;
using MassTransit;
using Nest;
using static System.Reflection.Metadata.BlobBuilder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigurationManager configuration = builder.Configuration;
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<IndexDataConsumer>();
    x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
    {
        cfg.Host(configuration["EventBusConnection"] );
        cfg.ReceiveEndpoint("IndexDataQueue", ep =>
        {
            ep.PrefetchCount = 16;
            ep.UseMessageRetry(r => r.Interval(20, 500));
            ep.ConfigureConsumer<IndexDataConsumer>(provider);
     
        });
    })); 
});

var pool = new SingleNodeConnectionPool(new Uri(configuration["ElasticSearchUri"]));
var settings = new ConnectionSettings(pool);
var client = new ElasticClient(settings);
builder.Services.AddSingleton(client); 
builder.Services.AddScoped(typeof(IIndexer), typeof(ESIndexer));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
