using Authentication.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.DataAccess
{
    public class UserManagementDBContext : DbContext
    {
        public UserManagementDBContext( DbContextOptions<UserManagementDBContext> options ) : base( options )
        {

        }

        public DbSet<User> Users { get; set; }

        public Task<int> SaveChanges( CancellationToken cancellationToken = default )
        {
            throw new NotImplementedException();
        }

        protected override void OnModelCreating( ModelBuilder builder )
        {
            builder.Entity<User>().Property( a => a.Email ).HasColumnName("email").IsRequired();
            builder.Entity<User>().Property( a => a.Name ).HasColumnName( "name" ).IsRequired();
            builder.Entity<User>().Property( a => a.EmailConfirmed ).HasColumnName( "email_confirmed" ).IsRequired().HasDefaultValue(false);
            builder.Entity<User>().Property( a => a.PasswordHash ).HasColumnName( "passwordHash" );
            builder.Entity<User>().Property( a => a.Salt ).HasColumnName( "salt" );
            builder.Entity<User>().HasKey( a => a.Id );
            builder.Entity<User>().Property( a => a.Id ).HasColumnName("id");
            builder.Entity<User>().HasIndex( a => a.Email ).IsUnique();
            builder.Entity<User>().ToTable( "users" );

            builder.Entity<Token>().Property( a => a.Id ).HasColumnName( "id" ).IsRequired();
            builder.Entity<Token>().HasIndex( a => a.Id ).IsUnique();
            builder.Entity<Token>().Property( a => a.UserId ).HasColumnName( "userid" ).IsRequired();
            builder.Entity<Token>().Property( a => a.Fingerprint ).HasColumnName( "fingerprint" ).IsRequired();
            builder.Entity<Token>().Property( a => a.UserAgent ).HasColumnName( "useragent" ).IsRequired();
            builder.Entity<Token>().HasIndex( a => new { a.UserId, a.Fingerprint } ).IsUnique();
            builder.Entity<Token>().ToTable( "tokens" );

            base.OnModelCreating( builder );
        }
    }
}