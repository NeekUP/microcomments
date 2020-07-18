using Authentication.Domain;
using Authentication.Usecases;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.DataAccess
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private UserManagementDBContext _context;
        private bool _disposed = false;

        public UnitOfWork( UserManagementDBContext context )
        {
            this._context = context;
        }

        private IUsersRepository usersRepository;
        private ITokensRepository tokensRepository;

        public IUsersRepository Users
        {
            get
            {
                if ( usersRepository == null )
                    usersRepository = new UsersRepository( _context );

                return usersRepository;
            }
        }

        public ITokensRepository Tokens
        {
            get
            {
                if ( tokensRepository == null )
                    tokensRepository = new TokensRepository( _context );

                return tokensRepository;
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch ( DbUpdateException ex )
            {
                var innerException = ex.InnerException as NpgsqlException;
                if ( ex.InnerException != null && innerException.ErrorCode == Constants.PgErrorUniqueViolation )
                {
                    throw new DuplicateItemExceptions($"{innerException.ErrorCode}:{innerException.Message}", ex);
                }
                else
                {
                    throw;
                }
            }
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( !_disposed )
            {
                if ( disposing )
                    _context.Dispose();
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }
    }
}
