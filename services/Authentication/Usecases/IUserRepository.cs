using Authentication.Domain;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface IUsersRepository : IGenericRepository<User>
    {
        Task<User> FindByEmailAsync( string email );
    }
}
