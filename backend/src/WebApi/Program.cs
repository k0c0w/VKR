using Microsoft.AspNetCore.Mvc;
using WebApi;
using WebApi.Endpoints.Map;

var builder = WebApplication.CreateSlimBuilder(args);
var app = ServiceRegistry.BuildApp(builder);

app.UseExceptionHandler();
app.UseMapEndpoints();

app.Run();
