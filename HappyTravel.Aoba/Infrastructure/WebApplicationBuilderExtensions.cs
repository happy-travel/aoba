using NATS.Client;

namespace HappyTravel.Aoba.Infrastructure;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        var endpoints = builder.Configuration.GetValue<string>("Nats:Endpoints").Split(";");
        
        builder.Services.AddNatsClient(options =>
        {
            options.Servers = endpoints;
            options.MaxReconnect = Options.ReconnectForever;
        }, ServiceLifetime.Singleton);
        
        // TODO: add vault and mail sender settings

        return builder;
    }
}