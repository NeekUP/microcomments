using Authentication.Domain;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Authentication.Infrastructure
{
    public class JwtTokenProvider : ITokenProvider
    {
        private readonly string _secret;
        private readonly ILogger _logger;

        private readonly IJwtEncoder _encoder;
        private readonly IJwtDecoder _decoder;
        public JwtTokenProvider( IOptions<JwtTokenProviderOptions> config, ILogger<JwtTokenProvider> logger )
        {
            _secret = config.Value.Secret ?? throw new ArgumentException( "secret not defined" );
            _logger = logger;

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtValidator validator = new JwtValidator( serializer, new UtcDateTimeProvider() );

            _encoder = new JwtEncoder( algorithm, serializer, urlEncoder );
            _decoder = new JwtDecoder( serializer, validator, urlEncoder, algorithm );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">'payload.Lifetime' will be set from 'lifetime'</param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public string Generate<T>( T payload, TimeSpan lifetime ) where T : TokenPayload
        {
            payload.Exp = GetUnixTime( lifetime );
            return _encoder.Encode( payload, _secret );
        }

        public T Read<T>( string token, bool verify )
        {
            try
            {
                return _decoder.DecodeToObject<T>( token, _secret, verify: verify );
            }
            catch( Exception ex )
            {
                _logger.LogError( ex, token );
                return default(T);
            }
        }

        private double GetUnixTime( TimeSpan add )
        {
            return UnixEpoch.GetSecondsSince( DateTimeOffset.UtcNow.Add( add ).ToUniversalTime() );
        }
    }
}
