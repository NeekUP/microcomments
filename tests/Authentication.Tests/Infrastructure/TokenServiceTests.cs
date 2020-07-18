using Authentication.Domain;
using Authentication.Infrastructure;
using Authentication.Infrastructure.DataAccess;
using Authentication.Usecases;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.Tests
{
    public class TokenServiceTests
    {
        [Fact]
        public async Task CreateToken_Created()
        {
            using UserManagementDBContext context = CreateDnContext();
            var db = CreateUnitOfWork( context );
            var tokenProvider = GetTokenProvider();
            var options = MockOptions( TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 10 ), 3 );

            TokenService service = new TokenService( db, tokenProvider, options, new NullLogger<TokenService>() );

            var userid = Guid.NewGuid();
            var fingerprint = Guid.NewGuid().ToString();
            var useragent = Guid.NewGuid().ToString();
            var (authToken, refreshToken) = await service.Create( userid, fingerprint, useragent );
            var dbRecords = await db.Tokens.GetAsync( a => a.UserId == userid );

            Assert.NotNull( authToken );
            Assert.NotNull( refreshToken );
            Assert.Single( dbRecords );
        }

        [Fact]
        public async Task RefreshToken_PreviousRemoved()
        {
            using UserManagementDBContext context = CreateDnContext();
            var db = CreateUnitOfWork( context );
            var tokenProvider = GetTokenProvider();
            var mock = new Mock<IOptions<TokenServiceOptions>>();
            var options = MockOptions( TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 10 ), 3 );

            TokenService service = new TokenService( db, tokenProvider, options, new NullLogger<TokenService>() );

            var userid = Guid.NewGuid();
            var fingerprint = Guid.NewGuid().ToString();
            var useragent = Guid.NewGuid().ToString();
            var (authToken, refreshToken) = await service.Create( userid, fingerprint, useragent );
            var dbRecords = await db.Tokens.GetAsync( a => a.UserId == userid );
            var firstExpireDate = dbRecords.First().ExpiredIn;

            var (refreshedAuthToken, refreshedRefreshToken) = await service.Refresh( authToken, refreshToken, fingerprint, useragent );
            var dbRecordsRefreshed = await db.Tokens.GetAsync( a => a.UserId == userid );

            Assert.NotNull( refreshedAuthToken );
            Assert.NotNull( refreshedRefreshToken );
            Assert.NotEqual( authToken, refreshedAuthToken );
            Assert.NotEqual( refreshToken, refreshedRefreshToken );
            Assert.Single( dbRecordsRefreshed );
            Assert.True( firstExpireDate < dbRecordsRefreshed.First().ExpiredIn );
        }

        [Fact]
        public async Task RefreshToken_AuthTokenExpired()
        {
            using UserManagementDBContext context = CreateDnContext();
            var db = CreateUnitOfWork( context );
            var tokenProvider = GetTokenProvider();
            var mock = new Mock<IOptions<TokenServiceOptions>>();
            var options = MockOptions( TimeSpan.FromSeconds( -1 ), TimeSpan.FromSeconds( 10 ), 3 );


            TokenService service = new TokenService( db, tokenProvider, options, new NullLogger<TokenService>() );

            var userid = Guid.NewGuid();
            var fingerprint = Guid.NewGuid().ToString();
            var useragent = Guid.NewGuid().ToString();
            var (authToken, refreshToken) = await service.Create( userid, fingerprint, useragent );
            var dbRecords = await db.Tokens.GetAsync( a => a.UserId == userid );

            var (refreshedAuthToken, refreshedRefreshToken) = await service.Refresh( authToken, refreshToken, fingerprint, useragent );
            var dbRecordsRefreshed = await db.Tokens.GetAsync( a => a.UserId == userid );

            Assert.NotNull( authToken );
            Assert.NotNull( refreshToken );
            Assert.NotNull( refreshedAuthToken );
            Assert.NotNull( refreshedRefreshToken );
            Assert.NotEqual( authToken, refreshedAuthToken );
            Assert.NotEqual( refreshToken, refreshedRefreshToken );
            Assert.Single( dbRecordsRefreshed );
            Assert.NotEqual( dbRecordsRefreshed.First().Id, dbRecords.First().Id );
        }

        [Fact]
        public async Task RefreshToken_RefreshTokenExpired()
        {
            using UserManagementDBContext context = CreateDnContext();
            var db = CreateUnitOfWork( context );
            var tokenProvider = GetTokenProvider();
            var mock = new Mock<IOptions<TokenServiceOptions>>();
            var options = MockOptions( TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( -1 ), 3 );

            TokenService service = new TokenService( db, tokenProvider, options, new NullLogger<TokenService>() );

            var userid = Guid.NewGuid();
            var fingerprint = Guid.NewGuid().ToString();
            var useragent = Guid.NewGuid().ToString();
            var (authToken, refreshToken) = await service.Create( userid, fingerprint, useragent );

            var (refreshedAuthToken, refreshedRefreshToken) = await service.Refresh( authToken, refreshToken, fingerprint, useragent );
            var dbRecordsRefreshed = await db.Tokens.GetAsync( a => a.UserId == userid );

            Assert.NotNull( authToken );
            Assert.NotNull( refreshToken );
            Assert.Null( refreshedAuthToken );
            Assert.Null( refreshedRefreshToken );
            Assert.Empty( dbRecordsRefreshed );
        }

        [Fact]
        public async Task CreateToken_MaxTokenCountReached()
        {
            using UserManagementDBContext context = CreateDnContext();
            var db = CreateUnitOfWork( context );
            var tokenProvider = GetTokenProvider();
            var mock = new Mock<IOptions<TokenServiceOptions>>();
            var options = MockOptions( TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 10 ), 3 );

            TokenService service = new TokenService( db, tokenProvider, options, new NullLogger<TokenService>() );

            var userid = Guid.NewGuid();
            var useragent = Guid.NewGuid().ToString();
            var (authToken1, refreshToken1) = await service.Create( userid, Guid.NewGuid().ToString(), useragent );
            Assert.Single(await db.Tokens.GetAsync( a => a.UserId == userid ));

            var (authToken2, refreshToken2) = await service.Create( userid, Guid.NewGuid().ToString(), useragent );
            Assert.Equal( 2, (await db.Tokens.GetAsync( a => a.UserId == userid )).Count() );

            var (authToken3, refreshToken3) = await service.Create( userid, Guid.NewGuid().ToString(), useragent );
            Assert.Equal( 3, (await db.Tokens.GetAsync( a => a.UserId == userid )).Count() );

            var (authToken4, refreshToken4) = await service.Create( userid, Guid.NewGuid().ToString(), useragent );
            Assert.Single( await db.Tokens.GetAsync( a => a.UserId == userid ) );
        }

        private UserManagementDBContext CreateDnContext()
        {
            var context = new UserManagementDBContext( GetDbContextOptions() );
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }

        private static IOptions<TokenServiceOptions> MockOptions( TimeSpan authLifetime, TimeSpan refreLifetime, int maxTokenCount )
        {
            var mock = new Mock<IOptions<TokenServiceOptions>>();
            mock.Setup( a => a.Value ).Returns( new TokenServiceOptions()
            {
                AuthLifetime = authLifetime,
                RefreshLifetime = refreLifetime,
                MaxUserTokenCount = maxTokenCount
            } );
            return mock.Object;
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

        private static ITokenProvider GetTokenProvider()
        {
            var optionsA = new Mock<IOptions<JwtTokenProviderOptions>>();
            optionsA.Setup( a => a.Value ).Returns( new JwtTokenProviderOptions() { Secret = Guid.NewGuid().ToString() } );
            var providerA = new JwtTokenProvider( optionsA.Object, new NullLogger<JwtTokenProvider>() );
            return providerA;
        }
    }
}
