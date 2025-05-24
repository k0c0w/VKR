using FluentMigrator.Runner;
using WebApi;

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

if (InProcessMigrationsAreOn(app.Configuration))
{
    TryMigrateOrExit(app.Services);
}

app.UseExceptionHandler();

app.UseRouting();
app.MapControllers();

app.Run();
return;

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

bool InProcessMigrationsAreOn(IConfiguration configuration) 
    => !string.IsNullOrEmpty(configuration["IN_PROCESS_MIGRATIONS_ON"]);