using System;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Beisen.Amqp
{
    [Serializable,XmlRoot("queue")]
    public class QueueSetting
    {
        [XmlAttribute("name")]
        public string QueueName { get; set; }
        [XmlAttribute("server")]
        public string Server { get; set; }
        [XmlAttribute("exchange")]
        public string Exchange { get; set; }
        //[XmlAttribute("route-key")]
        //public string RouteKey { get; set; }
        [XmlAttribute("durable")]
        public bool Durable { get; set; }
        [XmlAttribute("auto-delete")]
        public bool AutoDelete { get; set; }
        [XmlAttribute("need-ack")]
        public bool NeedAck { get; set; }
        [XmlAttribute("prefetch-size")]
        public int PrefetchSize { get; set; }
        [XmlAttribute("prefetch-count")]
        public int PrefetchCount { get; set; }
//        [XmlAttribute("concurrency-level")]
//        public int ConcurrencyLevel { get; set; }
    }
}