using System;

namespace Beisen.Amqp
{
    public class AmqpConnectionException:ApplicationException
    {
        public AmqpConnectionException() : base()
        {
            
        }

        public AmqpConnectionException(string host):
            base(string.Format("host {0} not reachable.",host))
        {
            
        }
        public AmqpConnectionException(string server, Exception exception)
            : base(string.Format("{0} not reachable.", server), exception)
        {
            
        }
    }

    public class AmqpConfigurationException : ApplicationException
    {
        public AmqpConfigurationException() : base()
        {
            
        }

        public AmqpConfigurationException(string message):
            base(message)
        {
            
        }
        public AmqpConfigurationException(string message, Exception innerException) :
            base(message, innerException)
        {
            
        }
        
    }
}