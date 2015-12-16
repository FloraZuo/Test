using System;
using System.Xml.Serialization;

namespace Beisen.Amqp
{
    [Serializable]
    public class ExchangeSetting
    {
        [XmlAttribute("server")]
        public string Server { get; set; }
        [XmlAttribute("name")]
        public string ExchangeName { get; set; }
        [XmlAttribute("type")]
        public ExchangeType ExchangeType { get; set; }
        [XmlAttribute("durable")]
        public bool Durable { get; set; }
        [XmlAttribute("autoDelete")]
        public bool AutoDelete { get; set; }
    }
}