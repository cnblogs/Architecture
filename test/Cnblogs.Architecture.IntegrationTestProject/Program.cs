using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection;
using Cnblogs.Architecture.Ddd.Cqrs.DependencyInjection.EventBus.Dapr;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;
using Cnblogs.Architecture.IntegrationTestProject.Application.Commands;
using Cnblogs.Architecture.IntegrationTestProject.Application.Queries;
using Cnblogs.Architecture.IntegrationTestProject.Payloads;
using Cnblogs.Architecture.TestIntegrationEvents;

const string appName = "test-web";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCqrs(typeof(Cnblogs.Architecture.IntegrationTestProject.Program).Assembly)
    .AddDefaultDateTimeAndRandomProvider();
builder.Services.AddDaprEventBus(appName);
builder.Services.AddControllers().AddCqrsModelBinderProvider();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddCnblogsApiVersioning();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Subscribe<TestIntegrationEvent>();

var apis = app.NewVersionedApi();
var v1 = apis.MapGroup("/api/v{version:apiVersion}").HasApiVersion(1);
v1.MapQuery<GetStringQuery>("strings/{id:int}");
v1.MapQuery<ListStringsQuery>("strings");
v1.MapCommand<CreateCommand>("strings", (CreatePayload payload) => new CreateCommand(payload.NeedError));
v1.MapCommand<UpdateCommand>(
    "strings/{id:int}",
    (int id, UpdatePayload payload) => new UpdateCommand(id, payload.NeedError));
v1.MapCommand<DeleteCommand>("strings/{id:int}");

app.Run();

namespace Cnblogs.Architecture.IntegrationTestProject
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class Program
    {
    }
}