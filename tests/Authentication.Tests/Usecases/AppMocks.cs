using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Authentication.Infrastructure.DataAccess;
using Authentication.Usecases;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Authentication.Tests
{
    public static class AppMocks
    {
        public static IUnitOfWork CreateInMemoryDB()
        {
            var context = new UserManagementDBContext( GetDbContextOptions() );
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return new UnitOfWork( context );
        }
        
        public static IPublishEndpoint MessagingPublisher()
        {
            var mock = new Mock<IPublishEndpoint>();
            mock.Setup(a => a.Publish(It.IsAny<object>(), CancellationToken.None)).Returns(Task.CompletedTask);
            return mock.Object;
        }

        private static DbContextOptions<UserManagementDBContext> GetDbContextOptions()
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
    }
}