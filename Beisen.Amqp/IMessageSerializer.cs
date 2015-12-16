namespace Beisen.Amqp
{
    public interface IMessageSerializer
    {
        object Deserialize(byte[] bytes);
        byte[] Serialize(object obj);
    }
}