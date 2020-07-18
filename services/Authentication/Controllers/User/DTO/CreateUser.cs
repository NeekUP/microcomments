using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Controllers.DTO
{
    public class CreateUserRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email
        {
            get { return _email.ToLower(); }
            set { _email = value; }
        }

        private string _email;

        [Required]
        [StringLength(64, MinimumLength = 5)]
        public string Password { get; set; }

        [Required]
        [StringLength( 128, MinimumLength = 32 )]
        public string Fingerprint { get; set; }
    }

    public class CreateUserResponse
    {
        public string AuthToken;
        public string RefreshToken;

        public CreateUserResponse( string authToken, string refreshToken)
        {
            AuthToken = authToken;
            RefreshToken = refreshToken;
        }
    }
}