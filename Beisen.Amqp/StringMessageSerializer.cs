using System.Text;

namespace Beisen.Amqp
{
    public class StringMessageSerializer : IMessageSerializer
    {
        public object Deserialize(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public byte[] Serialize(object obj)
        {
            return Encoding.UTF8.GetBytes(obj as string);
        }
    }
}