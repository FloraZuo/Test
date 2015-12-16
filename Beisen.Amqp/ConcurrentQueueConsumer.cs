using System.Collections.Generic;

namespace Beisen.Amqp
{
    public class ConcurrentQueueConsumer
    {
        public string QueueName { get; set; }
        public string RouteKey { get; set; }
        public IMessageHandler MessageHandler { get; set; }
        public int ConcurrencyLevel { get; set; }
        public int PrefetchSize { get; set; }
        public int PrefetchCount { get; set; }

        private List<QueueConsumer> _consumers;

        internal void Initialize()
        {
            _consumers = new List<QueueConsumer>(ConcurrencyLevel);
            for (var i = 0; i < ConcurrencyLevel; i++)
            {
                _consumers.Add(new QueueConsumer()
                               {
                                   QueueName = QueueName,
                                   MessageHandler = MessageHandler,
                                   RouteKey = RouteKey,
                                   PrefetchCount = PrefetchCount,
                                   PrefetchSize = PrefetchSize
                               });
            }
        }

        public void Start()
        {
            _consumers.ForEach(consumer => consumer.Start());
        }

        public void Stop()
        {
            _consumers.ForEach(consumer => consumer.Stop());
        }
    }
}