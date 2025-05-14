using FluentMigrator.Runner;
using WebApi;
using WebApi.Endpoints.Map;
using WebApi.Endpoints.Plans;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = ServiceRegistry.BuildApp(builder);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
}

app.UseExceptionHandler();
app.UseMapEndpoints();
app.UsePlansEndpoints();

if (InProcessMigrationsAreOn())
{
    TryMigrateOrExit(app.Services);
}

app.Run();

void TryMigrateOrExit(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();

    var migrator = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    var logger = scope.ServiceProvider.GetService<ILogger<IMigrationRunner>>();

    try
    {
        if (migrator.HasMigrationsToApplyUp())
        {
            migrator.MigrateUp();
        }
    }
    catch (Exception ex)
    {
        logger?.LogCritical(ex, "Failed to apply migrations: {message}", ex.Message);
        Environment.Exit(1);
    }
}

bool InProcessMigrationsAreOn() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("INPROCESS_MIGRATIONS_ON"));