using System.Diagnostics;
using HappyTravel.Aoba.Infrastructure;
using HappyTravel.Aoba.Models;
using HappyTravel.MailSender;

namespace HappyTravel.Aoba.Services;

public class SendMailService : ISendMailService
{
    public SendMailService(ILogger<SendMailService> logger, DiagnosticSource diagnosticSource, IMailSender mailSender)
    {
        _logger = logger;
        _diagnosticSource = diagnosticSource;
        _mailSender = mailSender;
    }


    public async Task SendMail(MailMessage? message)
    {
        using var activity = _diagnosticSource.StartActivity(new Activity(nameof(SendMail)), null);

        try
        {
            ArgumentNullException.ThrowIfNull(message);
            
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
    }


    private readonly ILogger<SendMailService> _logger;
    private readonly DiagnosticSource _diagnosticSource;
    private readonly IMailSender _mailSender;
}