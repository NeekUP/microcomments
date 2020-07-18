using Authentication.Domain;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface ITokensRepository : IGenericRepository<Token>
    {
        Task DeleteAsync( Expression<Func<Token, bool>> filter );
        Task<int> CountAsync( Expression<Func<Token, bool>> filter );
    }
}
