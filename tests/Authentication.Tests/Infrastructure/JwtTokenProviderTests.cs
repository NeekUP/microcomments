using Authentication.Domain;
using Authentication.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Authentication.Tests
{
    public class JwtTokenProviderTests
    {
        [Fact]
        public void ReadValidToken()
        {
            var provider = GetNewProvider();
            var message = Guid.NewGuid().ToString();
            var payload = new MessageTokenPayload( message );
            var token = provider.Generate<MessageTokenPayload>( payload, TimeSpan.FromSeconds(33) );

            var decoded = provider.Read<MessageTokenPayload>( token, verify: true );
            Assert.NotNull( decoded );
            Assert.Equal( payload.Message, decoded.Message );
        }

        [Fact]
        public void ReadNotValidToken_WrongSecret()
        {
            var providerA = GetNewProvider();
            var providerB = GetNewProvider();
            var messageB = Guid.NewGuid().ToString();
            var sourceB = new MessageTokenPayload( messageB );
            var tokenB = providerB.Generate<MessageTokenPayload>( sourceB, TimeSpan.FromSeconds( 33 ) );

            var decoded = providerA.Read<MessageTokenPayload>( tokenB, verify: true );

            Assert.Null( decoded );
        }

        [Fact]
        public void ReadNotValidToken_Expires()
        {
            var provider = GetNewProvider();
            var message = Guid.NewGuid().ToString();
            var payload = new MessageTokenPayload( message );
            var token = provider.Generate<MessageTokenPayload>( payload, TimeSpan.FromSeconds( -10 ) );

            var decoded = provider.Read<MessageTokenPayload>( token, verify: true );
            Assert.Null( decoded );
        }

        [Fact]
        public void ReadNotValidToken_Guid()
        {
            var provider = GetNewProvider();
            var token = Guid.NewGuid().ToString();

            var decoded = provider.Read<MessageTokenPayload>( token, verify: true );

            Assert.Null( decoded );
        }

        private static JwtTokenProvider GetNewProvider()
        {
            var optionsA = new Mock<IOptions<JwtTokenProviderOptions>>();
            optionsA.Setup( a => a.Value ).Returns( new JwtTokenProviderOptions() { Secret = Guid.NewGuid().ToString() } );
            var providerA = new JwtTokenProvider( optionsA.Object, new NullLogger<JwtTokenProvider>() );
            return providerA;
        }

        private class MessageTokenPayload : TokenPayload
        {
            public string Message { get; set; }

            public MessageTokenPayload( string message )
            {
                Message = message;
            }
        }
    }
}
