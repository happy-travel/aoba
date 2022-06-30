using Prometheus;

namespace HappyTravel.Aoba.Infrastructure;

public class Counters
{
    public static readonly Counter MailSentCounter = Prometheus.Metrics.CreateCounter(
         "aoba_mail_sent_counter",
        "Mail sent counter",
        new CounterConfiguration
        {
            LabelNames = new [] {"isSuccess"}
        });
}