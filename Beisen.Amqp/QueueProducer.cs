using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Beisen.Amqp
{
    public class QueueProducer:IDisposable
    {
        internal static  TaskFactory TaskFactory=new TaskFactory();
        public QueueProducer()
        {
            
        }
        public QueueProducer(string exchangeName,string routeKey)
        {
            Exchange = ServerStateManager.Instance.GetExchangeSetting(exchangeName);
            DefaultRouteKey = routeKey;
            Init();
        }
        public QueueProducer(string exchangeName)
            :this(exchangeName,"*")
        {
            
        }

        public void Init()
        {
            MaxWait = 100;
            try
            {
                _connection = ServerStateManager.Instance.CreateConnection(Exchange.Server);

                _model = _connection.CreateModel();
                _model.ExchangeDeclare(Exchange.ExchangeName, Exchange.ExchangeType.ToString(),
                                       Exchange.Durable, Exchange.AutoDelete, null);
            }
            catch (Exception error)
            {
                RuntimeInternal.Logger.Error(string.Format("Producer [{0}] binding to server failed.",Exchange.ExchangeName),error);
            }

        }

        public bool IsAlive
        {
            get { return Connection != null && Model != null && Connection.IsOpen && Model.IsOpen; }
        }

        private IModel _model;

        public IModel Model
        {
            get { return _model; }
        }

        private IConnection _connection;
        public IConnection Connection
        {
            get { return _connection; }
        }
        public string DefaultRouteKey { get; set; }
        public int MaxWait { get; set; }

        
        public ExchangeSetting Exchange { get; set; }

        public void Send(string routeKey, byte[] message, IBasicProperties basicProperties)
        {
            lock (this)
            {
                if (!IsAlive)
                {
                    Init();
                }
                if (!IsAlive)
                {
#if DEBUG
                    if (Connection != null)
                    {
                        Console.WriteLine(string.Join(",", Connection.ShutdownReport));
                    }
#endif
                    if (Connection != null)
                    {
                        RuntimeInternal.Logger.Error(string.Join(",", Connection.ShutdownReport));
                    }
                    else
                    {
                        throw new AmqpConnectionException(Exchange.Server, null);
            
                    }
                    
                }
                Model.BasicPublish(Exchange.ExchangeName, routeKey, basicProperties, message);
            }
        }

        public void Send(byte[] message)
        {
            Send(DefaultRouteKey,message,null/**/);

        }
        public void Send(string routeKey, byte[] message, IBasicProperties properties,bool async, Action<BasicGetResult> callback)
        {
            if (properties == null)
                properties = Model.CreateBasicProperties();
            var queueDeclareOk = Model.QueueDeclare(null, false, true, true, null);
            properties.ReplyTo = queueDeclareOk.QueueName;
//            _model.BasicPublish(Exchange.ExchangeName,routeKey,properties,message);
            Send(routeKey,message,properties);
            if (async)
            {
                TaskFactory
                    .StartNew(
                        () =>
                            {
                                var result=GetResult(queueDeclareOk.QueueName);
                                callback(result);
                            });
            }
            else
            {
                var result = GetResult(queueDeclareOk.QueueName);
                callback(result);

            }
            

        }
        public BasicGetResult GetResult(string queueName)
        {
            BasicGetResult result = null;
            int counter = 0;
            while (result == null && counter<MaxWait)
            {
                result = Model.BasicGet(queueName, false);
                if (result == null)
                {
                    Thread.Sleep(50);
                }
                counter++;
            }
            return result;
        }
        public void Send(byte[] message, bool async, Action<BasicGetResult> callback)
        {
            Send(DefaultRouteKey, message, null, async, callback);
        }
        public void Dispose()
        {
            _model.Dispose();
            _connection.Dispose();
        }
        
    }
}