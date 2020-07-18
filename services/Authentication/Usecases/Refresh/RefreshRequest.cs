using System.ComponentModel.DataAnnotations;

namespace Authentication.Usecases
{
    public class RefreshRequest : Validatable
    {
        [Required]
        public string AuthToken { get; }
        [Required]
        public string RefreshToken { get; }
        [Required]
        public string Fingerprint { get; }
        [Required]
        public string UserAgent { get; }

        public RefreshRequest( string authToken, string refreshToken, string fingerprint, string userAgent )
        {
            AuthToken = authToken;
            RefreshToken = refreshToken;
            Fingerprint = fingerprint;
            UserAgent = userAgent;
        }
    }
}
