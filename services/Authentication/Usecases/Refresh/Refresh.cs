using Authentication.Domain;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface IRefreshTokenHandler : IHandler<RefreshRequest, RefreshResponse> { }

    public class Refresh : IRefreshTokenHandler
    {
        private readonly ITokenService _tokenService;

        public Refresh( ITokenService tokenService )
        {
            _tokenService = tokenService;
        }

        public async Task<Result<RefreshResponse>> Handle( RefreshRequest model )
        {
            model.Validate();

            var tokenPair = await _tokenService.Refresh( model.AuthToken, model.RefreshToken, model.Fingerprint, model.UserAgent );
            if ( tokenPair == default )
                return Result<RefreshResponse>.Fail( Error.ACCESS_DENIED );

            return Result<RefreshResponse>.Ok( new RefreshResponse( tokenPair.authToken, tokenPair.refreshToken ) );
        }
    }
}
