using Authentication.Domain;

namespace Authentication.Usecases
{
    public class GetUserResponse
    {
        public User User { get; }

        public GetUserResponse( User user )
        {
            User = user;
        }
    }
}
