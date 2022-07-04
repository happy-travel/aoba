using HappyTravel.Aoba.Models;

namespace HappyTravel.Aoba.Services;

public interface ISendMailService
{
    Task SendMail(MailMessage? message);
}