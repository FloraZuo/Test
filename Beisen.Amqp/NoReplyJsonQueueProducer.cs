namespace Beisen.Amqp
{
    public class NoReplyJsonQueueProducer<TMessage> : JsonQueueProducer<TMessage, NoReply>
    {
        public NoReplyJsonQueueProducer(string exchange)
            : base(exchange)
        {

        }

        public NoReplyJsonQueueProducer(string exchange, string routeKey)
            : base(exchange, routeKey)
        {
        }
    }
}