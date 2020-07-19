using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Authentication.Domain;
using Authentication.Usecases;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Authentication.Tests
{
    public class ConfirmEmailTests
    {
        [Fact]
        public async Task ConfirmEmail_Success()
        {
            using var db = AppMocks.CreateInMemoryDB();
            var registerResult = await RegisterUser(db, "name", "email@email.com", "password");
            ConfirmEmail confirmEmail = new ConfirmEmail(db, new NullLogger<ConfirmEmail>());

            var dbUserBefore = await db.Users.GetByIDAsync(registerResult.Value.Id);
            Assert.False(dbUserBefore.EmailConfirmed);
            Assert.NotNull(dbUserBefore.EmailConfirmationSecret);
            
            var respnse = await confirmEmail.Handle(new ConfirmEmailRequest(registerResult.Value.Id, Guid.Parse(dbUserBefore.EmailConfirmationSecret)));
            Assert.True(respnse.Value.Confirmed);
            
            var dbUserAfter = await db.Users.GetByIDAsync(registerResult.Value.Id);
            Assert.True(dbUserAfter.EmailConfirmed);
            Assert.Null(dbUserAfter.EmailConfirmationSecret);
        }
        
        [Fact]
        public async Task ConfirmEmail_NotSuccess()
        {
            using var db = AppMocks.CreateInMemoryDB();
            var registerResult = await RegisterUser(db, "name", "email@email.com", "password");
            ConfirmEmail confirmEmail = new ConfirmEmail(db, new NullLogger<ConfirmEmail>());

            var dbUserBefore = await db.Users.GetByIDAsync(registerResult.Value.Id);
            Assert.False(dbUserBefore.EmailConfirmed);
            Assert.NotNull(dbUserBefore.EmailConfirmationSecret);
            
            await confirmEmail.Handle(new ConfirmEmailRequest(registerResult.Value.Id, Guid.NewGuid()));
            
            var dbUserAfter = await db.Users.GetByIDAsync(registerResult.Value.Id);
            Assert.False(dbUserBefore.EmailConfirmed);
            Assert.NotNull(dbUserBefore.EmailConfirmationSecret);
        }
        
        private async Task<Result<RegisterResponse>> RegisterUser(IUnitOfWork db, string name, string email, string password)
        {
            var dnsLookup = MockDnsLookup();
            var publisher = AppMocks.MessagingPublisher();
            var registerHandler = new Register(db, dnsLookup, MockHashProvider(), publisher, new NullLogger<Register>());
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