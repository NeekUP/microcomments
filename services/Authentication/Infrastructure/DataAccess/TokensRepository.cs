using Authentication.Domain;
using Authentication.Usecases;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.DataAccess
{
    public class TokensRepository : GenericRepository<Token>, ITokensRepository
    {
        public TokensRepository( UserManagementDBContext context ) : base( context )
        {
        }

        public async Task DeleteAsync( Expression<Func<Token, bool>> filter )
        {
            dbSet.RemoveRange( await GetAsync( filter ) );
        }

        public Task<int> CountAsync( Expression<Func<Token, bool>> filter )
        {
            return dbSet.CountAsync( filter );
        }
    }
}
