namespace Beisen.Amqp
{
    
    public interface IMessageHandler
    {
        IMessageSerializer Serializer { get; set; }
        //bool CanHandle(MessageContext context);
        MessageResult OnMessage(MessageContext context);
    }
}