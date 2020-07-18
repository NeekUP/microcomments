
using Authentication.Domain;
using Authentication.Usecases;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Infrastructure
{
    public class TokenService : ITokenService
    {
        private int _maxTokenCount;
        const string REFRESH_TOKEN_TYPE_ID = "r";
        const string AUTH_TOKEN_TYPE_ID = "a";

        private readonly ITokenProvider _tokenProvider;
        private readonly IUnitOfWork _repo;
        private readonly ILogger _logger;
        private readonly TimeSpan _authTokenLifetime;
        private readonly TimeSpan _refreshTokenLifetime;

        public TokenService( IUnitOfWork repo, ITokenProvider tokenProvider, IOptions<TokenServiceOptions> options, ILogger<TokenService> logger )
        {
            _tokenProvider = tokenProvider;
            _repo = repo;
            _logger = logger;
            _authTokenLifetime = options.Value.AuthLifetime;
            _refreshTokenLifetime = options.Value.RefreshLifetime;
            _maxTokenCount = options.Value.MaxUserTokenCount > 0 ? options.Value.MaxUserTokenCount : 10;
        }

        /// <summary>
        /// Create new token pair.
        ///     `authToken` for authentication (and authorization)
        ///     `refreshToken` for renewal authToken if client hasn't changes
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="fingerprint"></param>
        /// <param name="useragent"></param>
        /// <returns></returns>
        public async Task<(string authToken, string refreshToken)> Create( Guid userId, string fingerprint, string useragent )
        {
            var count = await _repo.Tokens.CountAsync( a => a.UserId == userId );

            if ( count >= _maxTokenCount )
                await _repo.Tokens.DeleteAsync( a => a.UserId == userId );
            else
                await _repo.Tokens.DeleteAsync( a => a.UserId == userId && a.Fingerprint == fingerprint );

            var id = Guid.NewGuid();
            string authToken = CreateAuthToken( userId, id, _authTokenLifetime );
            string refreshToken = CreateRefreshToken( id, _refreshTokenLifetime );

            _repo.Tokens.Insert( new Token( id, userId, fingerprint, useragent, DateTime.Now.Add( _refreshTokenLifetime ) ) );
            await _repo.SaveAsync();

            return (authToken, refreshToken);
        }

        /// <summary>
        /// Generate new authToken when it expired
        /// when `fingerprint` and `useragent` not changed since refreshToken was created.
        /// Drop all refreshTokens for User if `fingerprint` and `useragent` was changed
        /// or if token limit reached
        /// </summary>
        /// <param name="authToken">expired authToken</param>
        /// <param name="refreshToken"></param>
        /// <param name="fingerprint"></param>
        /// <param name="useragent"></param>
        /// <returns></returns>
        public async Task<(string authToken, string refreshToken)> Refresh( string authToken, string refreshToken, string fingerprint, string useragent )
        {
            var authPayload = _tokenProvider.Read<AuthTokenPayload>( authToken, verify: false );
            if ( authPayload == null )
            {
                _logger.LogError( "Fail to read authToken: {0}", authToken );
                return default;
            }

            if ( authPayload.Type != AUTH_TOKEN_TYPE_ID )
            {
                _logger.LogError( "AuthToken has wrong type id: {0}", authPayload.Type );
                return default;
            }

            var refreshPayload = _tokenProvider.Read<RefreshTokenPayload>( refreshToken, verify: true );
            if ( refreshPayload == null )
            {
                _logger.LogError( "Fail to read refreshToken: {0}", refreshToken );
                refreshPayload = _tokenProvider.Read<RefreshTokenPayload>( refreshToken, verify: false );
                if ( refreshPayload != null )
                    await DeleteTokenById( refreshPayload.RefreshId );
                else
                    await DeleteTokenById( authPayload.RefreshId );
                return default;
            }

            if ( refreshPayload.Type != REFRESH_TOKEN_TYPE_ID )
            {
                _logger.LogError( "RefreshToken has wrong type id: {0}", refreshPayload.Type );
                return default;
            }

            if ( authPayload.RefreshId != refreshPayload.RefreshId )
            {
                await DeleteTokenById( refreshPayload.RefreshId );
                await DeleteAllByUser( authPayload.UserId );
                _logger.LogError( "Fail to read refreshToken: {0}", refreshToken );
                return default;
            }

            var tokens = await _repo.Tokens.GetAsync(a => a.UserId == authPayload.UserId );

            var token = tokens.FirstOrDefault( a => a.Id == authPayload.RefreshId );
            if ( token == null )
                _logger.LogError( "Token with id {0} not found", refreshToken );
            else
                _repo.Tokens.Delete( token.Id );

            if ( token == null
                || token.Fingerprint != fingerprint
                || token.UserAgent != useragent
                || tokens.Count() > _maxTokenCount )
            {
                await DeleteAllByUser( authPayload.UserId );
            }

            await _repo.SaveAsync();

            return await Create( authPayload.UserId, fingerprint, useragent );
        }

        private async Task DeleteTokenById( Guid id )
        {
            _repo.Tokens.Delete( id );
            await _repo.SaveAsync();
        }

        private async Task DeleteAllByUser( Guid userId )
        {
            await _repo.Tokens.DeleteAsync( a => a.UserId == userId );
            await _repo.SaveAsync();
        }

        private string CreateRefreshToken( Guid id, TimeSpan lifetime )
        {
            var payload = new RefreshTokenPayload( id, REFRESH_TOKEN_TYPE_ID );
            var refreshToken = _tokenProvider.Generate( payload, lifetime );
            return refreshToken;
        }

        private string CreateAuthToken( Guid userId, Guid id , TimeSpan lifetime)
        {
            var payload = new AuthTokenPayload( userId, id, AUTH_TOKEN_TYPE_ID );
            var authToken = _tokenProvider.Generate( payload, lifetime );
            return authToken;
        }

        public class AuthTokenPayload : TokenPayload
        {
            public Guid UserId;
            public Guid RefreshId;
            public string Type;

            public AuthTokenPayload( Guid userId, Guid refreshId, string type )
            {
                UserId = userId;
                RefreshId = refreshId;
                Type = type;
            }
        }

        public class RefreshTokenPayload : TokenPayload
        {
            public Guid RefreshId;
            public string Type;

            public RefreshTokenPayload( Guid refreshId, string type )
            {
                RefreshId = refreshId;
                Type = type;
            }
        }
    }
}
