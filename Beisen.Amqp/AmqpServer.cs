using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Beisen.Amqp
{
    [Serializable,XmlRoot("server")]
    public class AmqpServer
    {
        [XmlAttribute("name")]
        public string ServerName { get; set; }
        /// <summary>
        /// AMQP server connection string
        /// <example>amqp://xud@bs</example>
        /// </summary>
        [XmlAttribute("uri")]
        public string Uri { get; set; }
        [XmlAttribute("user-name")]
        public string UserName { get; set; }
        [XmlAttribute("password")]
        public string Password { get; set; }
        [XmlAttribute("virtual-host")]
        public string VirtualHost { get; set; }
        [XmlElement("slave")]
        public List<string> SlaveServers { get; set; }

        internal List<string> GetServers()
        {
            var servers=new List<string>(3);
            if (!string.IsNullOrEmpty(Uri))
            {
                servers.Add(Uri);
            }
            if (SlaveServers != null)
            {
                servers.AddRange(SlaveServers);
            }
            return servers;
        } 
    }
}