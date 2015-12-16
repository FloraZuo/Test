using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Beisen.Configuration;

namespace Beisen.Amqp
{
    [XmlRoot("amqp-settings"),Serializable]
    public class AmqpSettings:BaseConfig<AmqpSettings>
    {
        [XmlArray("servers"),XmlArrayItem("server")]
        public List<AmqpServer> Servers { get; set; }
        [XmlArray("exchanges"),XmlArrayItem("exchange")]
        public List<ExchangeSetting> Exchanges { get; set; }
        [XmlArray("queues"),XmlArrayItem("queue")]
        public List<QueueSetting> Queues { get; set; } 
        
    }
}