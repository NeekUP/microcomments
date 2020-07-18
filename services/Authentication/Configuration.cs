using System;

namespace Authentication
{
    public class TokenServiceOptions
    {
        public TimeSpan AuthLifetime;
        public TimeSpan RefreshLifetime;
        public int MaxUserTokenCount;
    }

    public class JwtTokenProviderOptions
    {
        public string Secret;
    }

    public class RabbitMQOptions
    {
        public string Host { get; set; }
        public string VirtualHost { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
