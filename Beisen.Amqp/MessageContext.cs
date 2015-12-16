namespace Beisen.Amqp
{
    public class MessageContext
    {
        public QueueConsumer Consumer { get; set; }
        /// <summary>
        /// 当前包含的数据
        /// </summary>
        public virtual object Data { get; set; }
        /// <summary>
        /// 原始数据
        /// </summary>
        public virtual byte[] RawData { get; set; }
        /// <summary>
        /// 交换机
        /// </summary>
        public string Exchange { get; set; }
        /// <summary>
        /// 路由key
        /// </summary>
        public string RouteKey { get; set; }
        /// <summary>
        /// 投递标签
        /// </summary>
        public ulong DeliveryTag { get; set; }
        /// <summary>
        /// 是否重新投递(被其他Consumer退回)
        /// </summary>
        public string Redelivered { get; set; }
        /// <summary>
        /// 当前队列中的消息数
        /// </summary>
        public uint MessageCount { get; set; }
        /// <summary>
        /// 回调队列
        /// </summary>
        public string Callback { get; set; }

        public bool NeedCallback
        {
            get { return !string.IsNullOrEmpty(Callback); }
        }
    }
}