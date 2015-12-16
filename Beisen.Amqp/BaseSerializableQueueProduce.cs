using System;

namespace Beisen.Amqp
{
    public abstract class BaseSerializableQueueProducer<TMessage,TReply>:QueueProducer
    {
        
        protected BaseSerializableQueueProducer(string exchange):
            base(exchange)
        {
            NoReply = typeof (TReply) != typeof (NoReply);
        }
        
        protected BaseSerializableQueueProducer(string exchange, string routeKey):base(exchange,routeKey)
        {
            NoReply = typeof(TReply) != typeof(NoReply);
        }
        public IMessageSerializer MessageSerializer { get; set; }
        public IMessageSerializer ReplySerializer { get; set; }
        public bool NoReply { get; set; }
        public virtual void Send(string routeKey,TMessage message)
        {
            var bytes = MessageSerializer.Serialize(message);
            Send(routeKey,bytes,null);
        }
        public void Send(TMessage message)
        {
            Send(DefaultRouteKey,message);
        }

        public  virtual  void  Invoke(string routeKey, TMessage message, Action<TReply> callback)
        {
            var body = MessageSerializer.Serialize(message);
            Send(routeKey, body, null, true,
                 result =>
                     {
                         if (result != null)
                         {
                             var reply = (TReply) ReplySerializer.Deserialize(result.Body);
                             callback(reply);
                         }
                         else
                         {
                             callback(default(TReply));
                         }
                     });
        }
        public virtual void Invoke(TMessage message, Action<TReply> callback)
        {
            Invoke(DefaultRouteKey,message,callback);
        }
        /// <summary>
        /// 同步调用
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public TReply Invoke(string routeKey, TMessage message)
        {
            var body = MessageSerializer.Serialize(message);
            TReply reply =default(TReply);
            Send(routeKey, body, null, false,
                 result =>
                     {
                         if (result != null)
                         {
                             reply = (TReply)ReplySerializer.Deserialize(result.Body);
                         }
                     });
            return reply;
        }
        public TReply Invoke(TMessage message)
        {
            return Invoke(DefaultRouteKey, message);
        }

    }
}