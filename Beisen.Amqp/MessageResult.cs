namespace Beisen.Amqp
{
    public class MessageResult
    {
        public bool Quit { get; set; }
        public MessageStatus Status { get; set; }
        public bool ReQueue{get; set; }
        public object ReplyMessage { get; set; }
    }
}
