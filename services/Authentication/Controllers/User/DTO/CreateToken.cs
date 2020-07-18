using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Controllers.DTO
{
    public class CreateTokenRequest
    {
        [Required]
        [DataType( DataType.EmailAddress )]
        public string Email 
        {
            get { return _email.ToLower(); } 
            set { _email = value; } 
        }

        private string _email;
        [Required]
        [StringLength( 64, MinimumLength = 5 )]
        public string Password { get; set; }

        [Required]
        [StringLength( 128, MinimumLength = 32 )]
        public string Fingerprint { get; set; }
    }

    public class CreateTokenResponse
    {
        public string AuthToken;
        public string RefreshToken;

        public CreateTokenResponse( string authToken, string refreshToken )
        {
            AuthToken = authToken;
            RefreshToken = refreshToken;
        }
    }
}
