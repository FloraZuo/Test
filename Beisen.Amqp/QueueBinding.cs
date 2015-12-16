using RabbitMQ.Client;

namespace Beisen.Amqp
{
    public class QueueBinding
    {
        //public ConnectionFactory ConnectionFactory { get; set; }
        public ExchangeSetting Exchange { get; set; }
        public QueueSetting Queue { get; set; }
    }
}