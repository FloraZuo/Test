namespace Beisen.Amqp
{
    public class JsonQueueProducer<TMessage,TReply>:
        BaseSerializableQueueProducer<TMessage,TReply>
    {
        public JsonQueueProducer(string exchange) : base(exchange)
        {
            MessageSerializer = new JsonMessageSerializer<TMessage>();
            ReplySerializer = new JsonMessageSerializer<TReply>();
        }

        public JsonQueueProducer(string exchange, string routeKey) : base(exchange, routeKey)
        {
            MessageSerializer = new JsonMessageSerializer<TMessage>();
            ReplySerializer = new JsonMessageSerializer<TReply>();
        }
    }
}