using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Beisen.Configuration;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Beisen.Amqp.Testing
{
    [TestFixture]
    public class AmqpTests1
    {
        private AmqpSettings Settings;
        [TestFixtureSetUp]
        public void Setup()
        {
             Settings = new AmqpSettings()
                              {
                                  Servers = new List<AmqpServer>()
                                                {
                                                    new AmqpServer()
                                                    {
                                                        ServerName = "server1",
                                                        Uri="amqp://user1:user1@10.129.8.207:5672",
                                                        SlaveServers = new List<string>()
                                                        {
                                                            "amqp://user1:user2@10.129.8.207:5672",
                                                            "amqp://user1:user1@10.129.8.207:5672"
                                                        }
                                                    }
                                                },
                                  Exchanges = new List<ExchangeSetting>()
                                                  {
                                                      new ExchangeSetting()
                                                          {
                                                              Server = "server1",
                                                              AutoDelete = false,
                                                              Durable = true,
                                                              ExchangeName = "testExchange",
                                                              ExchangeType = ExchangeType.direct
                                                          },
                                                      new ExchangeSetting()
                                                          {
                                                              Server = "server1",
                                                              AutoDelete = false,
                                                              Durable = true,
                                                              ExchangeName = "testExchange",
                                                              ExchangeType = ExchangeType.direct
                                                          }
                                                  },

                                  Queues = new List<QueueSetting>()
                                               {
                                                   new QueueSetting()
                                                       {
                                                           Server="server1",
                                                           QueueName = "q1",
                                                           Exchange = "testExchange"
                                                       },
                                                   new QueueSetting()
                                                       {
                                                           Server="server1",
                                                           QueueName = "q2",
                                                           Exchange = "testExchange",
                                                           NeedAck = true
                                                       }
                                               }
                              };
        }
        [Test]
        public void SettingSerializeTest()
        {
            var serializer = new XmlSerializer(typeof(AmqpSettings));
            using (var fs = File.Create("config.xml"))
            {
                serializer.Serialize(fs,Settings);
            }
        }
        
        [Test]
        public void DictionaryTests()
        {
            var consts=new Dictionary<string, string>();
            consts.Add("a","a");
            consts.Add("b", "b");
            consts.Add("c","c");
            var constsArray =
                (
                    from kv in consts
                    select new {Key = kv.Key, Value = kv.Value}
                ).ToArray();
            var output = new Dictionary<string, IList<object>>();
            output.Add("const1",constsArray);
            var serializerSettings=new JsonSerializerSettings();
            var v = JsonConvert.SerializeObject(output, Formatting.Indented, serializerSettings);
            Console.WriteLine(v);
        }
        [Test]
        public void RpcTest()
        {
            ServerStateManager.Instance.Initialize(Settings);
            var producer=new NoReplyJsonQueueProducer<string>("testExchange","calc");
            var producerTask =
                new Task(
                    () =>
                        {
                            for (var i = 0; i < 1000; i++)
                            {
                                try
                                {
                                    Thread.Sleep(1000);
                                    producer.Send("test" + i.ToString());
                                }
                                catch (Exception error)
                                {
                                    Console.WriteLine(error);
                                }
                            }
                        });
            var consumer = QueueConsumerFactory.Create<TestMessageHandler>("q2", "calc");
            consumer.Start();
            producerTask.Start();
            producerTask.Wait();
            Thread.Sleep(10000);
            consumer.Stop();
        }
        [Test]
        public void RemoteConfig()
        {
            var producer = new NoReplyJsonQueueProducer<string>("recruit-app", "portal-sync");
            var producerTask =
                new Task(
                    () =>
                    {
                        for (var i = 0; i < 1000; i++)
                        {
                            try
                            {
                                Thread.Sleep(1000);
                                producer.Send("test" + i.ToString());
                            }
                            catch (Exception error)
                            {
                                Console.Write(error);
                            }
                        }
                    });
            var consumer = QueueConsumerFactory.Create<TestMessageHandler>("ProcessService", "portal-sync");
            consumer.Start();
            //producerTask.Start();
            //producerTask.Wait();
            Thread.Sleep(10000);
            consumer.Stop();
        }
        [Test]
        public void FactoryTest()
        {
            ServerStateManager.Instance.Initialize(Settings);
            var consumer=QueueConsumerFactory.Create<TestMessageHandler>("q2","calc");
            consumer.Start();
            var producer = new QueueProducer("testExchange");
            var jsonMessageSerializer=new JsonMessageSerializer<string>();
            producer.Send("calc",jsonMessageSerializer.Serialize("test1"), null);
            producer.Send("calc", jsonMessageSerializer.Serialize("test"), null);
            
            Thread.Sleep(new TimeSpan(0,0,1,0));
            consumer.Stop();
        }
        [Test]
        public void MultitenantTest()
        {
            QueueProducer producer = new QueueProducer("multitenant");
            producer.Send(Encoding.UTF8.GetBytes("Test1"));
        }
        [Test]
        public void MultitenantReceive()
        {
            var consumer=QueueConsumerFactory.Create<MultitenantHandler>("MetaObjectChangeService");
            consumer.Start();
            Thread.Sleep(10000);
        }
        internal class MultitenantHandler:StringBaseMessageHandler
        {
            public override MessageResult OnMessage(MessageContext context)
            {
                Console.WriteLine(context.Data.ToString());
                return NoAck();
            }
        }
        internal class TestMessageHandler:JsonBaseMessageHandler<string>
        {
            readonly Random _random=new Random();
            public override MessageResult OnMessage(MessageContext context)
            {
                Console.WriteLine(context.Data);
                return Ack();
                var n = _random.Next(10);
                //if(n>5)
                //    return Processed();
                //else
                //{
                //    return Rejected();
                //}
            }
            
        }
        
    }
}
