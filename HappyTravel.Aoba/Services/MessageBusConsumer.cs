using System.Text;
using HappyTravel.Aoba.Infrastructure;
using HappyTravel.Aoba.Models;
using NATS.Client;
using Newtonsoft.Json;

namespace HappyTravel.Aoba.Services;

public class MessageBusConsumer : BackgroundService
{
    public MessageBusConsumer(IConnection connection, ISendMailService sendMailService)
    {
        _connection = connection;
        _sendMailService = sendMailService;
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
            var message = JsonConvert.DeserializeObject<MailMessage>(Encoding.UTF8.GetString(args.Message.Data));
            await _sendMailService.SendMail(message);
        });
    }
    
    
    private readonly IConnection _connection;
    private readonly ISendMailService _sendMailService;
}