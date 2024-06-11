using Libby.Consumers;
using Libby.Data;
using Libby.Data.Sagas;
using Libby.Machines;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using Serilog.Events;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(
        Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341",
        apiKey: Environment.GetEnvironmentVariable("SEQ_API_KEY"))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

if (args.Length > 0 && args[0] == "migrate")
{
    Migrator.Migrate(builder.Configuration.GetConnectionString("Postgres") ?? throw new ArgumentNullException());
    return;
}

builder.Services.AddControllers();

var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("Postgres"));
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<LibbyDataContext>(
    opts => opts
        .UseNpgsql(dataSource)
        .UseSnakeCaseNamingConvention());

builder.Services.AddMassTransit(
    x =>
    {
        x.AddConfigureEndpointsCallback(
            (context, _, cfg) =>
            {
                cfg.UseEntityFrameworkOutbox<LibbyDataContext>(context);
            });

        x.AddEntityFrameworkOutbox<LibbyDataContext>(
            o =>
            {
                o.LockStatementProvider = new CustomPostgresLockStatementProvider();
                o.UseBusOutbox();
            });

        x.AddConsumer<FilesConsumer>();
        x.AddConsumer<LibrariesConsumer>();
        x.AddConsumer<LibraryItemsConsumer>();

        x.AddSagaStateMachine<LibraryScanMachine, LibraryScan, LibraryScanDefinition>();

        x.SetEntityFrameworkSagaRepositoryProvider(cfg =>
        {
            cfg.ConcurrencyMode = ConcurrencyMode.Optimistic;
            cfg.LockStatementProvider = new CustomPostgresLockStatementProvider();
            cfg.ExistingDbContext<LibbyDataContext>();
        });

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host("amqp://localhost");
            cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("libby"));
            cfg.UseDelayedMessageScheduler();
        });
    });

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.MapControllers();

await app.RunAsync();
