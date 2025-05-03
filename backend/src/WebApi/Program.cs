using WebApi;
using WebApi.Endpoints.Map;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = ServiceRegistry.BuildApp(builder);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseMapEndpoints();

app.Run();
