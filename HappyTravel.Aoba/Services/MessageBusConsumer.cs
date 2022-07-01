using System.Text.Json;
using HappyTravel.Aoba.Infrastructure;
using HappyTravel.Aoba.Models;
using NATS.Client;

namespace HappyTravel.Aoba.Services;

public class MessageBusConsumer : BackgroundService
{
    public MessageBusConsumer(IConnection connection, IMailSendService mailSendService)
    {
        _connection = connection;
        _mailSendService = mailSendService;
    }

    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection.SubscribeAsync(Constants.TopicKey, "queue", OnMailMessageReceived);
        return Task.CompletedTask;
    }


    private void OnMailMessageReceived(object? sender, MsgHandlerEventArgs args)
    {
        Task.Run(async () =>
        {
            var message = JsonSerializer.Deserialize<MailMessage>(args.Message.Data);
            await _mailSendService.SendMail(message);
        });
    }
    
    
    private readonly IConnection _connection;
    private readonly IMailSendService _mailSendService;
}