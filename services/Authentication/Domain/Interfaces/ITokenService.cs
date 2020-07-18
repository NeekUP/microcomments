using System;
using System.Threading.Tasks;

namespace Authentication.Domain
{
    public interface ITokenService
    {
        Task<(string authToken, string refreshToken)> Create( Guid userId, string fingerprint, string useragent );
        Task<(string authToken, string refreshToken)> Refresh( string authToken, string refreshToken, string fingerprint, string useragent );
    }
}