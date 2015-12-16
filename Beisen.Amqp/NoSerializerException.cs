using System;

namespace Beisen.Amqp
{
    public class NoSerializerException : ApplicationException
    {
        public NoSerializerException(Type type):base(type.FullName+" havn't serializer.")
        {
            
        }
    }
}