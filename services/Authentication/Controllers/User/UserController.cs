using Authentication.Controllers.DTO;
using Authentication.Usecases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace Authentication.Controllers
{
    [ApiController]
    [Produces( "application/json" )]
    [Consumes( "application/json" )]
    [Route( "api/v{version}" )]
    [ApiVersion( "1" )]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IServiceProvider _services;
        public UserController( IServiceProvider services, ILogger<UserController> logger )
        {
            _services = services;
            _logger = logger;
        }

        //[HttpGet( "user/{id:guid}" )]
        //[ProducesResponseType( StatusCodes.Status400BadRequest )]
        //public IEnumerable<int> Get( Guid id )
        //{
        //    throw new NotImplementedException();
        //}

        //[HttpGet( "users" )]
        //public IEnumerable<int> Get( [FromQuery] Guid[] id )
        //{
        //    throw new NotImplementedException();
        //}

        [HttpPost( "user" )]
        [ProducesResponseType( typeof( CreateTokenResponse ), StatusCodes.Status201Created )]
        [ProducesResponseType( StatusCodes.Status400BadRequest )]
        [ProducesResponseType( StatusCodes.Status500InternalServerError )]
        public async Task<ActionResult> CreateUser( [FromBody] CreateUserRequest request )
        {
            if ( !ModelState.IsValid )
                return BadRequest();

            try
            {
                var usecase = _services.GetRequiredService<IRegisterHandler>();
                var response = await usecase.Handle( new RegisterRequest( request.Name, request.Email, request.Password ) );
                if ( !response.Success )
                {
                    _logger.LogInformation( $"{nameof( CreateUser )}. {response.Error}. Name:{request.Name}, Email:{request.Email}" );
                    if ( response.Error == Error.EXISTS )
                        return Conflict();

                    return BadRequest();
                }

                return await CreateToken( new CreateTokenRequest() { Email = request.Email, Password = request.Password, Fingerprint = request.Fingerprint } );
            }
            catch ( ValidationException ex )
            {
                _logger.LogError( $"{nameof( CreateUser )}. Validation error: {ex.Data}" );
                return BadRequest();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, $"{nameof( CreateUser )} Name:{request.Name}, Email:{request.Email}, Password:{new string( '*', request.Password.Length )}" );
            }
            return StatusCode( (int)HttpStatusCode.InternalServerError );
        }

        [HttpPost( "token" )]
        [ProducesResponseType( typeof( CreateTokenRequest ), StatusCodes.Status201Created )]
        [ProducesResponseType( StatusCodes.Status400BadRequest )]
        [ProducesResponseType( StatusCodes.Status403Forbidden )]
        [ProducesResponseType( StatusCodes.Status500InternalServerError )]
        public async Task<ActionResult> CreateToken( [FromBody] CreateTokenRequest request )
        {
            if ( !ModelState.IsValid )
                return BadRequest();

            try
            {
                var usecase = _services.GetRequiredService<ILoginHandler>();
                var response = await usecase.Handle( new LoginRequest( request.Email, request.Password, request.Fingerprint, Request.Headers[HeaderNames.UserAgent] ) );
                if ( !response.Success )
                {
                    _logger.LogError( $"{nameof( CreateToken )}. Email:{request.Email}, Password:{new string( '*', request.Password.Length )}, Fingerprint:{request.Fingerprint}, UserAgent:{Request.Headers[HeaderNames.UserAgent]}" );
                    return Forbid();
                }

                return Created( Request.Path, new CreateTokenResponse( response.Value.AuthToken, response.Value.RefreshToken ) );
            }
            catch ( ValidationException ex )
            {
                _logger.LogError( $"{nameof( CreateToken )}. Validation error: {ex.Data}" );
                return BadRequest();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, $"{nameof( CreateToken )}. Email:{request.Email}, Password:{new string( '*', request.Password.Length )}, Fingerprint:{request.Fingerprint}, UserAgent:{Request.Headers[HeaderNames.UserAgent]}" );
            }
            return StatusCode( (int)HttpStatusCode.InternalServerError );
        }

        [HttpPost( "refresh" )]
        [ProducesResponseType( typeof( RefreshTokenResponse ), StatusCodes.Status201Created )]
        [ProducesResponseType( StatusCodes.Status400BadRequest )]
        [ProducesResponseType( StatusCodes.Status403Forbidden )]
        [ProducesResponseType( StatusCodes.Status500InternalServerError )]
        public async Task<ActionResult> RefreshToken( [FromBody] RefreshTokenRequest request )
        {
            if ( !ModelState.IsValid )
                return BadRequest();

            var usecase = _services.GetRequiredService<IRefreshTokenHandler>();
            var response = await usecase.Handle( new RefreshRequest( request.AuthToken, request.RefreshToken, request.Fingerprint, Request.Headers[HeaderNames.UserAgent] ) );
            if ( !response.Success )
            {
                _logger.LogError( $"{nameof( RefreshToken )}. AuthToken:{request.AuthToken}, RefreshToken:{request.RefreshToken}, Fingerprint:{request.Fingerprint}, UserAgent:{Request.Headers[HeaderNames.UserAgent]}" );
                return Forbid();
            }

            return Created( Request.Path, new RefreshTokenResponse( response.Value.AuthToken, response.Value.RefreshToken ) );
        }

        [HttpPatch( "confirm" )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        [ProducesResponseType( StatusCodes.Status400BadRequest )]
        [ProducesResponseType( StatusCodes.Status403Forbidden )]
        [ProducesResponseType( StatusCodes.Status500InternalServerError )]
        public async Task<ActionResult> ConfirmEmail( [FromBody] ConfirmEmailRequest request )
        {
            if ( !ModelState.IsValid )
                return BadRequest();

            try
            {
                var confirmation = _services.GetRequiredService<IConfirmationHandler>();
                var response = await confirmation.Handle( new ConfirmEmailRequest( request.Id, request.Secret ) );
                if ( !response.Success )
                {
                    _logger.LogError( $"{nameof( ConfirmEmail )}. {response.Error} UserId:{request.Id}, Secret:{request.Secret}" );
                    return Forbid();
                }

            }
            catch ( ValidationException ex )
            {
                _logger.LogError( $"{nameof( ConfirmEmail )}. Validation error: {ex.Data}" );
                return BadRequest();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, $"{nameof( ConfirmEmail )}. UserId:{request.Id}, Secret:{request.Secret}" );
            }
            return StatusCode( (int)HttpStatusCode.InternalServerError );
        }
    }
}
