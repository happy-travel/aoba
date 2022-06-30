using System.Diagnostics;
using System.Text.Json;
using HappyTravel.Aoba.Infrastructure;
using HappyTravel.Aoba.Models;
using HappyTravel.MailSender;
using NATS.Client;

namespace HappyTravel.Aoba.Services;

public class MessageBusConsumer : BackgroundService
{
    public MessageBusConsumer(IConnection connection, ILogger<MessageBusConsumer> logger, IMailSender mailSender, 
        DiagnosticSource diagnosticSource)
    {
        _connection = connection;
        _logger = logger;
        _mailSender = mailSender;
        _diagnosticSource = diagnosticSource;
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
            using var updaterActivity = _diagnosticSource.StartActivity(new Activity(nameof(OnMailMessageReceived)), null);
            
            try
            {
                var message = JsonSerializer.Deserialize<MailMessage>(args.Message.Data);
                var result = await _mailSender.Send(message.TemplateId, message.Recipients, message.Data);
                using var _ = _logger.BeginScope(new Dictionary<string, object>
                {
                    {"TemplateId", message.TemplateId},
                    {"Recipients", string.Join(';', message.Recipients)}
                });
                
                // TODO: add storing sent results to database

                if (result.IsFailure)
                {
                    _logger.LogError("Sent mail completed with error: {Error}", result.Error);
                    Counters.MailSentCounter.WithLabels(result.IsSuccess.ToString()).Inc();
                }
                else
                {
                    _logger.LogInformation("Sent mail successfully completed");
                    Counters.MailSentCounter.WithLabels(result.IsSuccess.ToString()).Inc();
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Sent mail failed");
            }
        });
    }
    
    
    private readonly IConnection _connection;
    private readonly ILogger<MessageBusConsumer> _logger;
    private readonly IMailSender _mailSender;
    private readonly DiagnosticSource _diagnosticSource;
}