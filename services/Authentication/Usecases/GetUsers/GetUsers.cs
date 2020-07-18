using System;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Usecases.GetUsers
{
    public interface IGetUserHandler : IHandler<GetUsersRequest, GetUsersResponse> { }

    public class GetUsers : IGetUserHandler
    {
        private readonly IUnitOfWork _repo;

        public GetUsers( IUnitOfWork repo )
        {
            _repo = repo;
        }

        public async Task<Result<GetUsersResponse>> Handle( GetUsersRequest model )
        {
            model.Validate();

            var users = await _repo.Users.GetAsync( a => model.Ids.Contains( a.Id ), asNoTracking: true );
            if ( users.Count() == 0 )
                return Result<GetUsersResponse>.Fail( Error.NOT_FOUND );

            return Result<GetUsersResponse>.Ok( new GetUsersResponse( users ) );
        }
    }
}
