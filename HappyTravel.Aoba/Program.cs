using HappyTravel.Aoba.Infrastructure;
using HappyTravel.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureInfrastructure(options =>
{
    options.ConsulKey = Constants.ConsulKey;
});
builder.ConfigureServices();

var app = builder.Build();
app.Run();