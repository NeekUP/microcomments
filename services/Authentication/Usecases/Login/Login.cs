using Authentication.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface ILoginHandler : IHandler<LoginRequest, LoginResponse> { }

    public class Login : ILoginHandler
    {
        private readonly IUnitOfWork _repo;
        private readonly IHashProvider _hashProvider;
        private readonly ITokenService _tokenService;

        public Login( IUnitOfWork repo, IHashProvider hashProvider, ITokenService tokenService )
        {
            _repo = repo;
            _hashProvider = hashProvider;
            _tokenService = tokenService;
        }

        public async Task<Result<LoginResponse>> Handle( LoginRequest model )
        {
            model.Validate();

            var user = (await _repo.Users.GetAsync( a => a.Email == model.Email )).FirstOrDefault();
            if ( user == null )
                return Result<LoginResponse>.Fail( Error.ACCESS_DENIED );

            if ( !user.IsValidPassword( model.Password, _hashProvider ) )
                return Result<LoginResponse>.Fail( Error.ACCESS_DENIED );

            var (authToken, refreshToken) = await _tokenService.Create( user.Id, model.Fingerprint, model.UserAgent );

            return Result<LoginResponse>.Ok( new LoginResponse( authToken, refreshToken ) );
        }
    }
}
