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
using System.Threading.Tasks;
using Xunit;

namespace Authentication.Tests
{
    public class LoginUsecaseTests
    {

        [Theory]
        // email
        [InlineData("email@email.com","123456","dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", true)]
        [InlineData("e@e.com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", true)]
        [InlineData("@e.com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("sdsd@.com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("sdsd@com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("sdsd.com", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("sdsdcom", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData("", "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        [InlineData(null, "123456", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false)]
        // password
        [InlineData( "email@email.com", "LongPassword64SymbolsLength----------LongPassword64SymbolsLength", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", true )]
        [InlineData( "email@email.com", "!@#$%^&*()_+/-~\"}{[]<>.,", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", true )]
        [InlineData( "email@email.com", "пароль с пробелом на русском", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", true )]
        [InlineData( "email@email.com", "", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false )]
        [InlineData( "email@email.com", "short", "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false )]
        [InlineData( "email@email.com", null, "dsdsdsd", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false )]
        // fingerprint
        [InlineData( "email@email.com", "123456", "", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false )]
        [InlineData( "email@email.com", "123456", null, "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:78.0) Gecko/20100101 Firefox/78.0", false )]
        // user-agent
        [InlineData( "email@email.com", "123456", "dsdsdsd", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36", true )]
        [InlineData( "email@email.com", "123456", "dsdsdsd", "Mozilla/5.0 (Macintosh; Intel Mac OS X x.y; rv:42.0) Gecko/20100101 Firefox/42.0", true )]
        [InlineData( "email@email.com", "123456", "dsdsdsd", "Opera/9.60 (Windows NT 6.0; U; en) Presto/2.1.1", true )]
        [InlineData( "email@email.com", "123456", "dsdsdsd", "Googlebot/2.1 (+http://www.google.com/bot.html)", true )]
        [InlineData( "email@email.com", "123456", "dsdsdsd", "", false )]
        [InlineData( "email@email.com", "123456", "dsdsdsd", null, false)]
        public async Task LoginValidation(string email, string password, string fingerprint, string useragent, bool isValid)
        {
            using ( var context = new UserManagementDBContext( GetDbContextOptions() ) )
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var db = CreateUnitOfWork( context );
                var hashProvider = MockHashProvider();
                var tokenService = MockTokenService();
                Login login = new Login( db, hashProvider, tokenService );
                var request = new LoginRequest( email, password, fingerprint, useragent );

                if ( isValid )
                {
                    var response = await login.Handle( request );
                    Assert.False( response.Success );
                    Assert.Equal( Error.ACCESS_DENIED, response.Error );
                }
                else
                {
                    await Assert.ThrowsAsync<ValidationException>( async () => await login.Handle( request ) );
                }
            }
        }

        private IUnitOfWork CreateUnitOfWork( UserManagementDBContext context )
        {
            return new UnitOfWork( context );
        }

        private DbContextOptions<UserManagementDBContext> GetDbContextOptions()
        {
            return new DbContextOptionsBuilder<UserManagementDBContext>()
                  .UseSqlite( CreateInMemoryDatabase() )
                  .Options;
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection( "Filename=:memory:" );
            connection.Open();
            return connection;
        }

        private IHashProvider MockHashProvider()
        {
            var mock = new Mock<IHashProvider>();
            mock.Setup( a => a.HashPassword( It.IsAny<string>() ) )
                .Returns<string>( a => (Encoding.UTF8.GetBytes( a ), Encoding.UTF8.GetBytes( a )) );

            return mock.Object;
        }

        private ITokenService MockTokenService()
        {
            var mock = new Mock<ITokenService>();
            mock.Setup( a => a.Create( It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>() ) )
                .Returns( Task.FromResult(("atoken","rtoken")) );

            return mock.Object;
        }
    }
}
