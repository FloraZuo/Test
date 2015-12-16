namespace Beisen.Amqp
{
    public abstract class BaseMessageHandler:IMessageHandler
    {
        public IMessageSerializer Serializer { get; set; }
        public abstract MessageResult OnMessage(MessageContext context);
        public MessageResult Reply(object replyObject)
        {
            return new MessageResult()
                       {
                           Status = MessageStatus.Ack,
                           ReplyMessage = replyObject
                       };
        }
        public MessageResult NoAck(bool requeue)
        {
            return new MessageResult()
                       {
                           Status = MessageStatus.NoAck,
                           ReQueue = requeue
                       };
        }
        public MessageResult NoAck()
        {
            return NoAck(true);
        }
        public MessageResult Ack(object data)
        {
            return new MessageResult()
                       {
                           Status = MessageStatus.Ack,
                           ReplyMessage = data
                       };
        }
        public MessageResult Ack()
        {
            return Ack(null);
        }
        public MessageResult Quit(MessageStatus status)
        {
            return new MessageResult()
                       {
                           Status = status,
                           Quit = true
                           
                       };
        }

        public MessageResult Reject(bool requeue=true)
        {
            return new MessageResult()
                       {
                           Status = MessageStatus.Reject,
                           ReQueue = requeue
                       };
        }
    }
}