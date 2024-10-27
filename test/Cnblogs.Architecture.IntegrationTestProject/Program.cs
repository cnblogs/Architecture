using System.Reflection;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.Ddd.EventBus.Abstractions;
using Cnblogs.Architecture.Ddd.EventBus.Dapr;
using Cnblogs.Architecture.IntegrationTestProject;
using Cnblogs.Architecture.IntegrationTestProject.Application.Commands;
using Cnblogs.Architecture.IntegrationTestProject.Application.Queries;
using Cnblogs.Architecture.IntegrationTestProject.Payloads;
using Cnblogs.Architecture.TestIntegrationEvents;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCqrs(Assembly.GetExecutingAssembly(), typeof(TestIntegrationEvent).Assembly)
    .AddLongToStringJsonConverter()
    .AddDefaultDateTimeAndRandomProvider()
    .AddEventBus(o => o.UseDapr(Constants.AppName));
builder.Services.AddControllers().AddCqrsModelBinderProvider().AddLongToStringJsonConverter();

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
v1.MapQuery<GetStringQuery>(
    "apps/{appId}/strings/{stringId:int}/value",
    MapNullableRouteParameter.Enable,
    enableHead: true);
v1.MapQuery(
    "strings/{stringId:int}",
    async (int stringId, [FromQuery] bool found = true)
        => await Task.FromResult(new GetStringQuery(StringId: stringId, Found: found)));
v1.MapQuery<ListStringsQuery>("strings");
v1.MapQuery<GetLongToStringQuery>("long-to-string/{id:long}");
v1.MapCommand<CreateLongToStringCommand>("long-to-string");
v1.MapCommand(
    "strings",
    (CreatePayload payload) => Task.FromResult(new CreateCommand(payload.NeedError, payload.Data)));
v1.MapCommand(
    "strings/{id:int}",
    (int id, UpdatePayload payload) => new UpdateCommand(id, payload.NeedValidationError, payload.NeedExecutionError));
v1.MapCommand<DeleteCommand>("strings/{id:int}");

// generic command map
v1.MapPostCommand<CreateCommand>("generic-map/strings");
v1.MapPutCommand<UpdateCommand>("generic-map/strings");
v1.MapDeleteCommand<DeleteCommand>("generic-map/strings/{id:int}");

app.Run();

namespace Cnblogs.Architecture.IntegrationTestProject
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class Program;
}
