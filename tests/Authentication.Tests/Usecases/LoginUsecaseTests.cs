using Authentication.Domain;
using Authentication.Infrastructure.DataAccess;
using Authentication.Usecases;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Authentication.Tests
{
    public class LoginUsecaseTests
    {
        [Theory]
        // email
        [InlineData("email@email.com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", true)]
        [InlineData("e@e.com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", true)]
        [InlineData("@e.com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("sdsd@.com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("sdsd@com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("sdsd.com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("sdsdcom", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData(null, "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        // password
        [InlineData("email@email.com", "LongPassword64SymbolsLength----------LongPassword64SymbolsLength", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", true)]
        [InlineData("email@email.com", "!@#$%^&*()_+/-~\"}{[]<>.,", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", true)]
        [InlineData("email@email.com", "пароль с пробелом на русском", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", true)]
        [InlineData("email@email.com", "", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("email@email.com", "short", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("email@email.com", null, "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        // fingerprint
        [InlineData("email@email.com", "123456", "", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("email@email.com", "123456", null, "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        // user-agent
        [InlineData("email@email.com", "123456", "dsdsdsd", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36", true)]
        [InlineData("email@email.com", "123456", "dsdsdsd", "Mozilla/5.0 (Macintosh; Intel Mac OS X x.y; rv:42.0) Gecko/20100101 Firefox/42.0", true)]
        [InlineData("email@email.com", "123456", "dsdsdsd", "Opera/9.60 (Windows NT 6.0; U; en) Presto/2.1.1", true)]
        [InlineData("email@email.com", "123456", "dsdsdsd", "Googlebot/2.1 (+http://www.google.com/bot.html)", true)]
        [InlineData("email@email.com", "123456", "dsdsdsd", "", false)]
        [InlineData("email@email.com", "123456", "dsdsdsd", null, false)]
        public async Task LoginValidation(string email, string password, string fingerprint, string useragent, bool isValid)
        {
            using var db = AppMocks.CreateInMemoryDB();
            var hashProvider = MockHashProvider();
            var tokenService = MockTokenService();
            Login login = new Login(db, hashProvider, tokenService);
            var request = new LoginRequest(email, password, fingerprint, useragent);

            if (isValid)
            {
                var response = await login.Handle(request);
                Assert.False(response.Success);
                Assert.Equal(Error.ACCESS_DENIED, response.Error);
            }
            else
            {
                await Assert.ThrowsAsync<ValidationException>(async () => await login.Handle(request));
            }
        }

        [Fact]
        public async Task Login_NotValidPassword()
        {
            using var db = AppMocks.CreateInMemoryDB();
            var hashProvider = MockHashProvider();
            var tokenService = MockTokenService();
            var email = "email@emal.com";
            var password = "123456q!";
            var fingerprint = "123123123";
            var useragent = "12312312312";
            var regResult = await RegisterUser(db, hashProvider,Guid.NewGuid().ToString(), email, password);
            Assert.True(regResult.Success);
            Assert.True(regResult.Success);

            var loginHandler = new Login(db, hashProvider, tokenService);
            var wrongPassword = "123123123132";
            var request = new LoginRequest(email, wrongPassword, fingerprint, useragent);

            var response = await loginHandler.Handle(request);
            Assert.False(response.Success);
            Assert.Equal(Error.ACCESS_DENIED, response.Error);
        }

        [Fact]
        public async Task Login_Success()
        {
            using var db = AppMocks.CreateInMemoryDB();
            var hashProvider = MockHashProvider();
            var tokenService = MockTokenService();

            var email = "email@emal.com";
            var password = "123456q!";
            var fingerprint = "123123123";
            var useragent = "12312312312";
            var regResult = await RegisterUser(db, hashProvider, Guid.NewGuid().ToString(),email, password);
            Assert.True(regResult.Success);

            var loginHandler = new Login(db, hashProvider, tokenService);
            var request = new LoginRequest(email, password, fingerprint, useragent);

            var response = await loginHandler.Handle(request);
            Assert.True(response.Success);
            Assert.NotNull(response.Value.AuthToken);
            Assert.NotNull(response.Value.RefreshToken);
        }

        private async Task<Result<RegisterResponse>> RegisterUser(IUnitOfWork db, IHashProvider hashProvider, string name, string email, string password)
        {
            var dnsLookup = MockDnsLookup();
            var publisher = AppMocks.MessagingPublisher();
            var registerHandler = new Register(db, dnsLookup, hashProvider, publisher, new NullLogger<Register>());
            var regRequest = new RegisterRequest( name , email, password);
            var regResult = await registerHandler.Handle(regRequest);
            return regResult;
        }

        private IDnsLookup MockDnsLookup()
        {
            var mock = new Mock<IDnsLookup>();
            mock.Setup(a => a.QueryMX(It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<string>>(new string[] {"127.0.0.1"}));

            return mock.Object;
        }

        private IHashProvider MockHashProvider()
        {
            var mock = new Mock<IHashProvider>();
            mock.Setup(a => a.HashPassword(It.IsAny<string>()))
                .Returns<string>(a => (Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(a)));

            mock.Setup(a => a.Hash(It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .Returns<byte[], byte[]>((pass, salt) => pass);
            return mock.Object;
        }

        private ITokenService MockTokenService()
        {
            var mock = new Mock<ITokenService>();
            mock.Setup(a => a.Create(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(("atoken", "rtoken")));

            return mock.Object;
        }
    }
}