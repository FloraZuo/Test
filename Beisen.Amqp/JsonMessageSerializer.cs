using System.Text;
using Newtonsoft.Json;

namespace Beisen.Amqp
{
    public class JsonMessageSerializer<T> : IMessageSerializer
    {
        private static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings();

        static JsonMessageSerializer()
        {
            _serializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            _serializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }

        public object Deserialize(byte[] bytes)
        {
            string ctx = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(ctx, _serializerSettings);
        }

        public byte[] Serialize(object obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, _serializerSettings));
        }
    }
}