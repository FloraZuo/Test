using System;

namespace Beisen.Amqp
{
    public static class QueueConsumerFactory
    {

        public static QueueConsumer Create(string queueName, string routeKey, IMessageHandler handler,
                                           bool startImmediately,int prefetchSize=0,int prefetchCount=10
            )
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException("queueName");
            if (handler == null)
                throw new ArgumentNullException("handler");
            var consumer = new QueueConsumer
                               {
                                   QueueName = queueName, 
                                   RouteKey = routeKey, 
                                   MessageHandler = handler,
                                   PrefetchSize = prefetchSize,
                                   PrefetchCount = prefetchCount
                               };
            if (startImmediately)
                consumer.Start();
            return consumer;
        }

        public static ConcurrentQueueConsumer Create(string queueName, string routeKey, IMessageHandler handler,
            int concurrencyLevel, bool startImmediately, int prefetchSize = 0, int prefetchCount = 10)
        {
            if(string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException("queueName");
            if(handler==null)
                throw new ArgumentNullException("handler");
            if(concurrencyLevel<=0)
                throw new ArgumentOutOfRangeException("concurrencyLevel");

            var concurrentConsumer = new ConcurrentQueueConsumer()
                                     {
                                         QueueName = queueName,
                                         RouteKey = routeKey,
                                         MessageHandler = handler,
                                         ConcurrencyLevel = concurrencyLevel,
                                         PrefetchSize = prefetchSize,
                                         PrefetchCount = prefetchCount
                                     };
            concurrentConsumer.Initialize();
            if(startImmediately)
                concurrentConsumer.Start();
            return concurrentConsumer;

        }
        /// <summary>
        /// 默认创建，但不开始接收消息
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="routeKey"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static QueueConsumer Create(string queueName, string routeKey, IMessageHandler handler)
        {
            return Create(queueName, routeKey, handler, false);
        }

        public static ConcurrentQueueConsumer Create(string queueName, string routeKey, IMessageHandler handler,
            int concurrencyLevel)
        {
            return Create(queueName, routeKey, handler, concurrencyLevel, false);
        }

        public static QueueConsumer Create(string queueName, IMessageHandler handler)
        {
            return Create(queueName, null /*routeKey*/, handler);
        }
        public static ConcurrentQueueConsumer Create(string queueName, IMessageHandler handler,int concurrencyLevel)
        {
            return Create(queueName, null /*routeKey*/, handler,concurrencyLevel);
        }
        public static QueueConsumer Create(string queueName)
        {
            return Create(queueName, null);
        }
        public static ConcurrentQueueConsumer Create(string queueName,int concurrencyLevel)
        {
            return Create(queueName, null,concurrencyLevel);
        }
        public static QueueConsumer Create<THandler>(string queueName, string routeKey)
            where THandler:IMessageHandler,new()
        {
            var handler = new THandler();
            if (handler.Serializer == null)
                throw new NoSerializerException(typeof (THandler));
            return Create(queueName, routeKey, handler);
        }

        public static ConcurrentQueueConsumer Create<THandler>(string queueName, string routeKey,int concurrencyLevel)
            where THandler : IMessageHandler, new()
        {
            var handler = new THandler();
            if (handler.Serializer == null)
                throw new NoSerializerException(typeof(THandler));
            return Create(queueName, routeKey, handler,concurrencyLevel);
        }
        public static QueueConsumer Create<THandler>(string queueName)
            where THandler : IMessageHandler, new()
        {
            return Create<THandler>(queueName, null /*routeKey*/);
        }
        public static ConcurrentQueueConsumer Create<THandler>(string queueName,int concurrencyLevel)
            where THandler : IMessageHandler, new()
        {
            return Create<THandler>(queueName, null /*routeKey*/,concurrencyLevel);
        }
       
       
    }
}
