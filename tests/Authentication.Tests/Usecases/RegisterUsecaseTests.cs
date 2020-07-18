using Authentication.Domain;
using Authentication.Infrastructure.DataAccess;
using Authentication.Usecases;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.Tests
{
    public class RegisterUsecaseTests
    {
        [Theory]
        [InlineData( "", "qq@qq.com", "123456", false )]
        [InlineData( "qwerty0", "", "qwerty", false )]
        [InlineData( "qwerty1", "qq1@qq.com", "", false )]
        [InlineData( "12345", "qq2@qq.com", "123456", false )]
        [InlineData( "qwerty2", "qq3@qq.com", "123456", true )]
        [InlineData( "qwerty3", "qq4@qq.com", "qwerty", true )]
        [InlineData( "qwerty4", "qq5@qqcom", "qwerty", false )]
        [InlineData( "qwerty5", "11qq.com", "qwerty", false )]
        [InlineData( "qwerty6", "абвгд@qq.com", "qwerty", true )]
        [InlineData( "qwerty7", "qq6@абвгд.com", "qwerty", true )]
        [InlineData( "qwerty8", "qq7@абвгд.фыв", "qwerty", true )]
        [InlineData( "qwerty9", "qq8@qq.com", "йцукен", true )]
        [InlineData( "йцукен1", "qq9@qq.com", "qwerty", true )]
        [InlineData( "йцу кен", "qq10@qq.com", "qwerty", false )]
        [InlineData( "йцукен2", "qq11@qq.com", "qwe rty", true )]
        [InlineData( "йцукен3", "qq12 qq@qq.com", "qwerty", false )]
        [InlineData( "йцук\r\nен3", "qq13qq@qq.com", "qwerty", false )]
        [InlineData( "йцукен4", "qq13qq@qq.com", "qwerty\n", false )]
        [InlineData( "йцукен5", "\r\nqq14qq@qq.com", "qwerty\n", false )]
        [InlineData( "йцукен6", "John@Gıthub.com", "qwerty", true)]
        [InlineData( "йцукен6", "John@Github.com", "qwerty", true)]
        public async Task RegisterValidation( string name, string email, string password, bool isValid )
        {
            using ( var context = new UserManagementDBContext( GetDbContextOptions() ) )
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var db = CreateUnitOfWork( context );
                var dnsLookup = MockDnsLookup();
                var hashProvider = MockHashProvider();
                var messagPublisher = MockMessagingPublisher();
                var register = new Register( db, dnsLookup, hashProvider, messagPublisher, new NullLogger<Register>() );
                var request = new RegisterRequest( name, email, password );

                if ( isValid )
                {
                    var response = await register.Handle( request );

                    Assert.True( response.Success );
                    Assert.Null( response.Error );
                    Assert.NotNull( response.Value );
                    Assert.NotNull( response.Value.Name );
                    Assert.NotEqual( Guid.Empty, response.Value.Id );
                }
                else
                {
                    await Assert.ThrowsAsync<ValidationException>( async () => await register.Handle( request ) );
                }
            }
        }

        // TODO: add pg database for testing exception
        [Fact]
        public async Task RegisterDuplicateEmail()
        {
            var name = "namename";
            var email = "valid@email.com";
            var password = "validpassword";

            var dnsLookup = MockDnsLookup();
            var hashProvider = MockHashProvider();
            var messagPublisher = MockMessagingPublisher();

            using ( var context = new UserManagementDBContext( GetDbContextOptions() ) )
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                Register register = new Register( CreateUnitOfWork( context ), dnsLookup, hashProvider, messagPublisher, new NullLogger<Register>() );

                // First register
                var response = await register.Handle( new RegisterRequest( name, email, password ) );
                Assert.True( response.Success );
                Assert.Null( response.Error );
                Assert.NotNull( response.Value );
                Assert.NotNull( response.Value.Name );
                Assert.NotEqual( Guid.Empty, response.Value.Id );

                // Second register
                await Assert.ThrowsAsync<SqliteException>( async () => await register.Handle( new RegisterRequest( name, email, password ) ) );
            }
        }

        private IUnitOfWork CreateUnitOfWork( UserManagementDBContext context)
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

        private IDnsLookup MockDnsLookup()
        {
            var mock = new Mock<IDnsLookup>();
            mock.Setup( a => a.QueryMX( It.IsAny<string>() ) )
                .Returns( Task.FromResult<IEnumerable<string>>( new string[] { "127.0.0.1" } ) );

            return mock.Object;
        }

        private IHashProvider MockHashProvider()
        {
            var mock = new Mock<IHashProvider>();
            mock.Setup( a => a.HashPassword( It.IsAny<string>() ) )
                .Returns<string>( a => (Encoding.UTF8.GetBytes( a ), Encoding.UTF8.GetBytes( a )) );

            return mock.Object;
        }

        private IPublishEndpoint MockMessagingPublisher()
        {
            var mock = new Mock<IPublishEndpoint>();
            mock.Setup( a => a.Publish( It.IsAny<object>(), CancellationToken.None ) ).Returns( Task.CompletedTask );
            return mock.Object;
        }
    }
}
