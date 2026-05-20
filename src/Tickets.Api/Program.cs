using System.Text.Json.Serialization;
using Marten;
using Marten.Events.Projections;
using Tickets.Api.Infrastructure;
using Tickets.Api.Projections;
using Weasel.Core;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddMarten(opts =>
{
    var conn = builder.Configuration.GetConnectionString("Postgres")
               ?? "Host=localhost;Port=5432;Database=tickets;Username=tickets;Password=tickets";
    opts.Connection(conn);
    opts.UseSystemTextJsonForSerialization(enumStorage: EnumStorage.AsString);
    opts.Events.StreamIdentity = Marten.Events.StreamIdentity.AsGuid;
    opts.Projections.Add<TicketSummaryProjection>(ProjectionLifecycle.Inline);
})
.IntegrateWithWolverine()
.UseLightweightSessions();

builder.Host.UseWolverine();

builder.Services.AddWolverineHttp();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddExceptionHandler<InvalidTicketCommandExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.MapWolverineEndpoints();

app.Run();

public partial class Program;
