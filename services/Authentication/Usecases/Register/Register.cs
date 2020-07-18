using Authentication.Domain;
using Authentication.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface IRegisterHandler : IHandler<RegisterRequest, RegisterResponse> { }

    public class Register : IRegisterHandler
    {
        private readonly IUnitOfWork _repo;
        private readonly IDnsLookup _dnsLookup;
        private readonly IHashProvider _hashProvider;
        private readonly ILogger _logger;
        private readonly IPublishEndpoint _publisher;

        public Register( IUnitOfWork repo, IDnsLookup dnsLookup, IHashProvider hashProvider, IPublishEndpoint publisher, ILogger<Register> logger )
        {
            _repo = repo;
            _dnsLookup = dnsLookup;
            _hashProvider = hashProvider;
            _logger = logger;
            _publisher = publisher;
        }

        public async Task<Result<RegisterResponse>> Handle( RegisterRequest model )
        {
            model.Validate();

            var err = await Check( model );
            if ( err != Error.NONE )
                return Result<RegisterResponse>.Fail( err );

            var user = new User( model.Email, model.Name, model.Password, _hashProvider )
            {
                EmailConfirmationSecret = Guid.NewGuid().ToString( "N" )
            };

            _repo.Users.Insert( user );

            try
            {
                await _repo.SaveAsync();
            }
            catch ( DuplicateItemExceptions )
            {
                return Result<RegisterResponse>.Fail( Error.EXISTS );
            }

            await _publisher.Publish( new UserRegistered( user ) );

            return Result<RegisterResponse>.Ok( new RegisterResponse( user.Id, user.Name ) );
        }

        private async Task<Error> Check( RegisterRequest model )
        {
            if ( !await IsEmailHostExists( model ) )
                return Error.EMAIL_HOST_UNREACHABLE;

            if ( await IsUserExists( model ) )
                return Error.EXISTS;

            return Error.NONE;
        }

        private async Task<bool> IsEmailHostExists( RegisterRequest model )
        {
            var emailHost = GetEmailHost( model.Email );
            var isEmailHostExists = await HostHasMX( emailHost );
            return isEmailHostExists;
        }

        private async Task<bool> IsUserExists( RegisterRequest model )
        {
            return await _repo.Users.FindByEmailAsync( model.Email ) != null;
        }

        private async Task<bool> HostHasMX( string host )
        {
            return (await _dnsLookup.QueryMX( host )).Count() > 0;
        }

        private string GetEmailHost( string email )
        {
            return email.Split( '@' )[1];
        }
    }
}