using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface IGetUserHandler : IHandler<GetUserRequest, GetUserResponse> { }

    public class GetUser
    {
        private readonly IUnitOfWork _repo;

        public GetUser( IUnitOfWork repo )
        {
            _repo = repo;
        }

        public async Task<Result<GetUserResponse>> Handle( GetUserRequest model )
        {
            model.Validate();

            var users = await _repo.Users.GetAsync(a => a.Id == model.Id, asNoTracking: true );
            if ( users.Count() == 0 )
                return Result<GetUserResponse>.Fail( Error.NOT_FOUND );

            return Result<GetUserResponse>.Ok( new GetUserResponse( users.First() ) );
        }
    }
}
