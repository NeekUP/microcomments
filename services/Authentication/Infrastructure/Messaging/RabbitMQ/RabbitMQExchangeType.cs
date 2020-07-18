namespace Authentication.Infrastructure.Messaging.RabbitMQ
{
    public static class RabbitMQExchangeType
    {
        public const string Direct = "direct";
        public const string Fanout = "fanout";
        public const string Topic = "topic";
    }
}
