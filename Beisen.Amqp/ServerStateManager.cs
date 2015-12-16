using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Beisen.Amqp
{
    public class ServerStateManager
    {
        static ServerStateManager()
        {
            Instance=new ServerStateManager();
            Instance.Initialize(AmqpSettings.Instance);
            AmqpSettings.ConfigChanged += AmqpSettingsConfigChanged;
        }

        static void AmqpSettingsConfigChanged(object sender, System.EventArgs e)
        {
            Instance.Initialize(AmqpSettings.Instance);
        }
        public static ServerStateManager Instance { get; private set; }
        public void Reset()
        {

        
        }
        public void Initialize(AmqpSettings settings)
        {
            if (settings == null )
                return;
            if (settings.Servers == null || settings.Exchanges == null)
                return;
            ConnectionFactories=new ConcurrentDictionary<string, List<ConnectionFactory>>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var server in settings.Servers)
            {
                var serverUris = server.GetServers();
                if (serverUris.Count == 0)
                {
                    throw new AmqpConfigurationException("server configuration name [" + server.ServerName +
                                                         "] have no any endpoint.");
                }
                var connectionFactories = new List<ConnectionFactory>(serverUris.Count);

                foreach (var serverUri in serverUris)
                {
                    connectionFactories.Add(new ConnectionFactory()
                    {
                        Uri = serverUri
                    });
                }
                ConnectionFactories.AddOrUpdate(server.ServerName, connectionFactories,
                    (name, conn) => connectionFactories);
            }
            ExchangeSettings=new ConcurrentDictionary<string, ExchangeSetting>(StringComparer.OrdinalIgnoreCase);
            foreach (var exchange in settings.Exchanges)
            {
                ExchangeSettings.AddOrUpdate(exchange.ExchangeName, exchange, (name, ex) => ex);
            }
            QueueSettings =new ConcurrentDictionary<string, QueueSetting>(StringComparer.OrdinalIgnoreCase);
            foreach (var queue in settings.Queues)
            {
                QueueSettings.AddOrUpdate(queue.QueueName, queue, (n, q) => q);
            }
                
        }

        

        public ConcurrentDictionary<string, List<ConnectionFactory>> ConnectionFactories { get; private set; }
        public ConcurrentDictionary<string, ExchangeSetting> ExchangeSettings { get; private set; }
        public ConcurrentDictionary<string, QueueSetting> QueueSettings { get; private set; }
        public ExchangeSetting GetExchangeSetting(string exchangeName)
        {
            ExchangeSetting exchangeSetting = null;
            if (ExchangeSettings.TryGetValue(exchangeName, out exchangeSetting))
                return exchangeSetting;
            else
                throw new KeyNotFoundException(string.Format("exchange [{0}] configuration not found",exchangeName));
        }
        private List<ConnectionFactory> GetConnectionFactory(string serverName)
        {
            if (!ConnectionFactories.ContainsKey(serverName))
            {
                throw  new ArgumentOutOfRangeException("serverName",String.Format("server {0} not found in config file.",serverName));
            }
            return ConnectionFactories[serverName];
        }
        static readonly Random ServerRandom=new Random();

        static IConnection TryConnectTo(List<ConnectionFactory> factories)
        {
            
            var firstServerIndex = ServerRandom.Next(factories.Count);
            IConnection connection = null;
            ConnectionFactory currentFactory = null;
            currentFactory = factories[firstServerIndex];
            try
            {
                connection = currentFactory.CreateConnection();
                if (connection == null || !(connection.IsOpen))
                {
                    connection = null;
                    throw new AmqpConnectionException(currentFactory.HostName);
                }
            }
            catch (Exception error)
            {
                RuntimeInternal.Logger.Error("connect to random server", error);
                for (var i = 0; i < factories.Count; i++)
                {
                    if (i == firstServerIndex)
                        continue;
                    try
                    {
                        connection= factories[i].CreateConnection();
                        if (connection != null && connection.IsOpen)
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        RuntimeInternal.Logger.Error(e);
                    }
                }

            }
            return connection;
        }
        public IConnection CreateConnection(string serverName)
        {
            var connectionFactories = GetConnectionFactory(serverName);
            IConnection connection = null;
            connection = TryConnectTo(connectionFactories);
            if (connection == null || (!connection.IsOpen ))
            {
                throw new NoAvailableServerException(serverName);
            }
            return connection;
        }

        public QueueBinding GetQueueBinding(string queueName)
       {
           if (!QueueSettings.ContainsKey(queueName))
           {
               throw new ArgumentOutOfRangeException("queueName",string.Format("queue {0} config not found in config file.",queueName));
           }
          var binding=new QueueBinding();
          var queueSetting= QueueSettings[queueName];
           if (queueSetting != null)
           {
               binding.Queue = queueSetting;

               if (!string.IsNullOrEmpty(queueSetting.Exchange))
               {
                   binding.Exchange = ExchangeSettings[queueSetting.Exchange];
                   //if (string.IsNullOrEmpty(binding.Exchange.Server))
                   //{
                   //    binding.ConnectionFactory = ConnectionFactories[binding.Exchange.Server];
                   //}
               }
               //if (!string.IsNullOrEmpty(queueSetting.Server))
               //{
               //    binding.ConnectionFactory = ConnectionFactories[queueSetting.Server];
               //}
           }
           return binding;

       }
    }

    public class NoAvailableServerException : ApplicationException
    {
        public NoAvailableServerException(string serverName):
            base("config node ["+serverName +"] have no any available server." )
        {
            

        }
    }
}