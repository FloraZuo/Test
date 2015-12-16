namespace Beisen.Amqp
{
    public class StringQueueProducer<TMessage, TReply> : BaseSerializableQueueProducer<TMessage, TReply>
    {
        public StringQueueProducer(string exchange) : base(exchange)
        {
            MessageSerializer=new StringMessageSerializer();
            ReplySerializer=new StringMessageSerializer();
        }

        public StringQueueProducer(string exchange, string routeKey) : base(exchange, routeKey)
        {
            MessageSerializer = new StringMessageSerializer();
            ReplySerializer = new StringMessageSerializer();
        }
    }
}