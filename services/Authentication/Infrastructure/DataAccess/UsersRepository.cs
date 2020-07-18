using Authentication.Domain;
using Authentication.Usecases;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.DataAccess
{
    public class UsersRepository : GenericRepository<User>, IUsersRepository
    {
        public UsersRepository( UserManagementDBContext context ) : base( context )
        {
        }

        public async Task<User> FindByEmailAsync( string email )
        {
            IdnMapping idn = new IdnMapping();
            var punyCode = idn.GetAscii( email.ToLower() );
            return ( await GetAsync( a => a.NormalizedEmail == punyCode )).FirstOrDefault();
        }
    }
}
