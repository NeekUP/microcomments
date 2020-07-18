using System.ComponentModel.DataAnnotations;

namespace Authentication.Controllers.DTO
{
    public class RefreshTokenRequest
    {
        [Required]
        public string AuthToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        [Required]
        [StringLength( 128, MinimumLength = 32 )]
        public string Fingerprint { get; set; }
    }

    public class RefreshTokenResponse
    {
        public string AuthToken;
        public string RefreshToken;

        public RefreshTokenResponse( string authToken, string refreshToken )
        {
            AuthToken = authToken;
            RefreshToken = refreshToken;
        }
    }
}
