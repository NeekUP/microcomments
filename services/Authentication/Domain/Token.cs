using System;

namespace Authentication.Domain
{
    public class Token
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Fingerprint { get; private set; }
        public string UserAgent { get; private set; }
        public DateTime ExpiredIn { get; private set; }

        public Token( Guid id, Guid userId, string fingerprint, string userAgent, DateTime expiredIn )
        {
            Id = id;
            UserId = userId;
            Fingerprint = fingerprint;
            UserAgent = userAgent;
            ExpiredIn = expiredIn;
        }
    }
}
