using HappyTravel.Aoba.Models;

namespace HappyTravel.Aoba.Services;

public interface IMailSendService
{
    Task SendMail(MailMessage? message);
}