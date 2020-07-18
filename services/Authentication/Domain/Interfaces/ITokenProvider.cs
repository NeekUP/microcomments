using Newtonsoft.Json;
using System;

namespace Authentication.Domain
{
    public interface ITokenProvider
    {
        string Generate<T>( T claims, TimeSpan expiration ) where T : TokenPayload;
        T Read<T>( string token, bool verify );
    }

    public class TokenPayload
    {
        [JsonProperty("exp")]
        public double Exp { get; set; }
    }
}