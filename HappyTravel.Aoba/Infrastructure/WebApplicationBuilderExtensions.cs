using HappyTravel.Aoba.Services;
using HappyTravel.MailSender;
using HappyTravel.MailSender.Infrastructure;
using HappyTravel.MailSender.Models;
using HappyTravel.VaultClient;
using NATS.Client;

namespace HappyTravel.Aoba.Infrastructure;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        #region Add Nats

        var endpoints = builder.Configuration.GetValue<string>("Nats:Endpoints").Split(";");
        
        builder.Services.AddNatsClient(options =>
        {
            options.Servers = endpoints;
            options.MaxReconnect = Options.ReconnectForever;
        }, ServiceLifetime.Singleton);

        #endregion

        #region Add SendGrid sender

        using var vaultClient = new VaultClient.VaultClient(new VaultOptions
        {
            Engine = builder.Configuration["Vault:Engine"],
            Role = builder.Configuration["Vault:Role"],
            BaseUrl = new Uri(builder.Configuration[builder.Configuration["Vault:Endpoint"]])
        });
        vaultClient.Login(builder.Configuration[builder.Configuration["Vault:Token"]]).Wait();
        
        var mailSettings = vaultClient.Get(builder.Configuration["MailOptions"]).GetAwaiter().GetResult();
        builder.Services.Configure<SenderOptions>(options =>
        {
            options.ApiKey = mailSettings["apiKey"];
            options.SenderAddress = new EmailAddress(mailSettings["senderAddress"]);
            options.BaseUrl = new Uri(mailSettings["baseUrl"]);
        });

        builder.Services.AddSingleton<IMailSender, SendGridMailSender>();
        builder.Services.AddHttpClient(SendGridMailSender.HttpClientName)
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        #endregion

        builder.Services.AddHealthChecks();
        builder.Services.AddHostedService<MessageBusConsumer>();
        builder.Services.AddTransient<IMailSendService, MailSendService>();

        return builder;
    }
}