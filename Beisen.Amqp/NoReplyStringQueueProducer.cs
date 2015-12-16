namespace Beisen.Amqp
{
    public class NoReplyStringQueueProducer<TMessage> : StringQueueProducer<TMessage, NoReply>
    {
       
        public NoReplyStringQueueProducer(string exchange) : base(exchange)
        {
        }

        public NoReplyStringQueueProducer(string exchange, string routeKey) : base(exchange, routeKey)
        {
        }
    }
}