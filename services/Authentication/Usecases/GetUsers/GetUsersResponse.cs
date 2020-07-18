using Authentication.Domain;
using System.Collections.Generic;

namespace Authentication.Usecases
{
    public class GetUsersResponse
    {
        public IEnumerable<User> Users { get; }

        public GetUsersResponse( IEnumerable<User> users )
        {
            Users = users;
        }
    }
}
