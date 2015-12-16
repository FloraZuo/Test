namespace Beisen.Amqp
{
    public abstract class JsonBaseMessageHandler<T> : BaseMessageHandler
    {
        protected JsonBaseMessageHandler()
        {
            Serializer = new JsonMessageSerializer<T>();
        }
        
    }
    public abstract class StringBaseMessageHandler:BaseMessageHandler
    {
        protected StringBaseMessageHandler()
        {
            Serializer = new StringMessageSerializer();
        }
    }
}