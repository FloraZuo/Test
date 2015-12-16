using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Beisen.Amqp
{
    public class QueueConsumer:DefaultBasicConsumer
    {

        internal static class ConnectionHolder
        {
            static ConnectionHolder()
            {
                _consumers=new BlockingCollection<QueueConsumer>();
                AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            }

            private static System.Collections.Concurrent.BlockingCollection<QueueConsumer> _consumers; 
            static void CurrentDomain_DomainUnload(object sender, EventArgs e)
            {
                
                
            }
        }
        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }
        public bool Quit { get; set; }
        public IConnection Connection { get; set; }
        public IModel Channel { get; set; }
        public IMessageHandler MessageHandler { get; set; }
        public QueueBinding Binding { get; set; }
        public string RouteKey { get; set; }
        protected Task CurrentTask = null;
        protected bool ContinueConsume;
        protected QueueDeclareOk QueueDeclareResult;
        public bool IgnoreRedelivered { get; set; }
        public int PrefetchSize { get; set; }
        public int PrefetchCount { get; set; }
        public void Start()
        {
            Stop();
            MakeQueueBinding();
            ContinueConsume = true;
            CurrentTask=new Task(ConsumeEntry);
            CurrentTask.Start();
        }

        internal void MakeQueueBinding()
        {
            try
            {
                Binding = ServerStateManager.Instance.GetQueueBinding(QueueName);
                if (Binding == null)
                {
                    throw new AmqpConfigurationException(string.Format("queue config [{0}] not exists.", QueueName));
                }
                Connection = ServerStateManager.Instance.CreateConnection(Binding.Exchange.Server);// Binding.ConnectionFactory.CreateConnection();
                Channel = Connection.CreateModel(); 
                QueueDeclareResult = Channel.QueueDeclare(Binding.Queue.QueueName, Binding.Queue.Durable, false,
                                                          Binding.Queue.AutoDelete, null);
                if (Binding.Exchange != null)
                {
                    Channel.ExchangeDeclare(Binding.Exchange.ExchangeName, Binding.Exchange.ExchangeType.ToString(),
                                            Binding.Exchange.Durable, Binding.Exchange.AutoDelete, null);
                    Channel.QueueBind(QueueDeclareResult.QueueName, Binding.Queue.Exchange, RouteKey ?? "*");

                    int prefetchSize = Binding.Queue.PrefetchSize == default(int)
                        ? PrefetchSize
                        : Binding.Queue.PrefetchSize;
                    
                    int prefetchCount = Binding.Queue.PrefetchCount == default(int)
                        ? PrefetchCount
                        : Binding.Queue.PrefetchCount;
                    prefetchCount = prefetchCount == default(int) ? 10 : prefetchCount;
                    
                    Channel.BasicQos((uint)prefetchSize,(ushort)prefetchCount,false);

                    Channel.BasicConsume(QueueDeclareResult.QueueName, !Binding.Queue.NeedAck,this);
                }
            }
            catch (Exception error)
            {
                RuntimeInternal.Logger.Error("binding to server failed.",error);
            }
        }
        public override void HandleBasicConsumeOk(string consumerTag)
        {
            RuntimeInternal.Logger.Info("basic consume ok on "+consumerTag);
            base.HandleBasicConsumeOk(consumerTag);
        }
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange,
                                                string routingKey, IBasicProperties properties, byte[] body)
        {
            try
            {

                var context =
                    new MessageContext
                        {
                            DeliveryTag = deliveryTag,
                            RawData = body,
                            Consumer = this,
                            Exchange = exchange,
                            RouteKey = routingKey,
                            Data = MessageHandler.Serializer.Deserialize(body),
                            MessageCount = 0,
                            Callback = properties.ReplyTo
                        };
                var result = MessageHandler.OnMessage(context);


                switch (result.Status)
                {
                    case MessageStatus.Ack:
                        if (Binding.Queue.NeedAck)
                            Channel.BasicAck(deliveryTag, false);
                        break;
                    case MessageStatus.NoAck:
                        if (Binding.Queue.NeedAck)
                            Channel.BasicNack(deliveryTag, false, result.ReQueue);
                        break;
                    case MessageStatus.Reject:
                        Channel.BasicReject(deliveryTag, result.ReQueue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (context.NeedCallback)
                {
                    //todo:Create response
                }
                if (result.Quit)
                    ContinueConsume = false;

            }
            catch (Exception error)
            {

#if DEBUG
                Console.Error.WriteLine(error.StackTrace);
                RuntimeInternal.Logger.Error("Consume message error.",error);
#endif
            }
        }

        public bool IsAlive
        {
            get { return Connection!=null && Channel!=null && Connection.IsOpen && Channel.IsOpen; }
        }
        private void ConsumeEntry()
        {
            while (ContinueConsume)
            {
                try
                {
                    Thread.Sleep(5000);
                    if (!IsAlive)
                    {
                        MakeQueueBinding();
                    }
                   
                    /*
                    var basicGetResult = Channel.BasicGet(QueueDeclareResult.QueueName, !Binding.Queue.NeedAck);
                    
                    if (basicGetResult == null)
                    {
#if DEBUG
                        //Console.Error.WriteLine("result is null");
#endif
                        Thread.Sleep(100);
                        continue;

                    }
                    var context =
                        new MessageContext
                            {
                                DeliveryTag = basicGetResult.DeliveryTag,
                                RawData = basicGetResult.Body,
                                Consumer = this,
                                Exchange = basicGetResult.Exchange,
                                RouteKey = basicGetResult.RoutingKey,
                                Data = MessageHandler.Serializer.Deserialize(basicGetResult.Body),
                                MessageCount=basicGetResult.MessageCount,
                                Callback = basicGetResult.BasicProperties.ReplyTo
                            };
                    var result = MessageHandler.OnMessage(context);


                    switch (result.Status)
                    {
                        case MessageStatus.Ack:
                            if(Binding.Queue.NeedAck)
                                Channel.BasicAck(basicGetResult.DeliveryTag, false);
                            break;
                        case MessageStatus.NoAck:
                            if(Binding.Queue.NeedAck)
                                Channel.BasicNack(basicGetResult.DeliveryTag,false,result.ReQueue);
                            break;
                        case MessageStatus.Reject:
                            Channel.BasicReject(basicGetResult.DeliveryTag,result.ReQueue);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    if (context.NeedCallback)
                    {
                        
                    }
                    if (result.Quit)
                        ContinueConsume = false;
*/
                }
                catch (Exception error)
                {

#if DEBUG
                    Console.Error.WriteLine(error.StackTrace);
#endif
                    RuntimeInternal.Logger.Fatal("connect to amqp server failed.",error);
                    break;
                }
                     
            }
        }
        public void Stop()
        {
            Stop(new TimeSpan(0,0,0,5));
        }
        public void Stop(TimeSpan waitTime)
        {
            if(Connection==null || Channel==null)
                return;
            try
            {
                ContinueConsume = false;
                CurrentTask.Wait(waitTime);
                Channel.Close();
                Connection.Close();
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
            }
            finally
            {
                Channel = null;
                Connection = null;
            }
        }
        #region old method
        //        public void Consume(string routeKey,Func<QueueConsumer, MessageContext, MessageResult> callback)
//        {
//            var binding = ServerStateManager.Instance.GetQueueBinding(QueueName);
//            using (var conection=binding.ConnectionFactory.CreateConnection())
//            {
//               using (var model = conection.CreateModel())
//               {
                   
//                   var declareOk = model.QueueDeclare(binding.Queue.QueueName, binding.Queue.Durable, false, binding.Queue.AutoDelete, null);
//                   if (binding.Exchange != null)
//                   {
//                       model.ExchangeDeclare(binding.Exchange.ExchangeName, binding.Exchange.ExchangeType.ToString(), binding.Exchange.Durable, binding.Exchange.AutoDelete, null);
//                       model.QueueBind(declareOk.QueueName, binding.Queue.Exchange, routeKey);
//                   }
                   
//                   while (!Quit)
//                   {
//                       try
//                       {
//                           var basicGetResult = model.BasicGet(declareOk.QueueName, !binding.Queue.NeedAck);

//                           if (basicGetResult == null)
//                           {
//#if DEBUG
//                               Console.Error.WriteLine("result is empty.");
//#endif
//                               Thread.Sleep(100);
//                               continue;
                               
//                           }
//                           var context = new MessageContext
//                                             {
//                                                 DeliveryTag = basicGetResult.DeliveryTag,
//                                                 RawData = basicGetResult.Body,
//                                                 Consumer = this,
//                                                 Exchange = basicGetResult.Exchange,
//                                                 RouteKey = basicGetResult.RoutingKey
//                                             };
//                           MessageResult result = callback(this, context);

//                           if (binding.Queue.NeedAck)
//                           {
//                               if (result.Status==MessageStatus.Processed)
//                               {
//                                   model.BasicAck(basicGetResult.DeliveryTag, false);
//                               }
//                               else
//                               {
//                                   model.BasicNack(basicGetResult.DeliveryTag,false,true);
//                               }
//                           }
//                           if (result.Status==MessageStatus.Quit || result.Status==MessageStatus.QuitWithProcessed || result.Status==MessageStatus.QuitWithReject)
//                           {
//                               break;
//                           }

//                       }
//                       catch (Exception error)
//                       {
                           
//#if DEBUG
//                           Console.Error.WriteLine(error.StackTrace);
//#endif
//                           break;
//                       }
//                   }
//               }
//            }
        //        }
        #endregion
    }
}