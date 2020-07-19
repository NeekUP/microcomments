using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface IConfirmationHandler : IHandler<ConfirmEmailRequest, ConfirmEmailResponse> { }

    public class ConfirmEmail : IConfirmationHandler
    {
        private readonly IUnitOfWork _repo;
        private readonly ILogger _logger;

        public ConfirmEmail( IUnitOfWork repo, ILogger<ConfirmEmail> logger )
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ConfirmEmailResponse>> Handle( ConfirmEmailRequest model )
        {
            model.Validate();

            var user = await _repo.Users.GetByIDAsync( model.Id );
            if ( user == null )
                return Result<ConfirmEmailResponse>.Fail( Error.NOT_FOUND );

            if( user.EmailConfirmed )
                return Result<ConfirmEmailResponse>.Ok( new ConfirmEmailResponse( true ) );

            if ( user.IsEmailConfirmationValid(model.Secret) )
                return Result<ConfirmEmailResponse>.Fail( Error.FORBIDDEN );

            user.MarkEmailAsConfirmed();

            try
            {
                await _repo.SaveAsync();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, $"UserId: {user.Id}" );
                return Result<ConfirmEmailResponse>.Fail( Error.INTERNAL_ERROR );
            }

            return Result<ConfirmEmailResponse>.Ok( new ConfirmEmailResponse( true ) );
        }
    }
}
