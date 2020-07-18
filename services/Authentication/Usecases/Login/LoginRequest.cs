using Authentication.Utils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Usecases
{
    public class LoginRequest : Validatable
    {
        [Required]
        [UserEmail()]
        public string Email { get; }

        [Required]
        [StringLength(64,MinimumLength = 6)]
        public string Password { get; }

        [Required]
        public string Fingerprint { get; }

        [Required]
        public string UserAgent { get; }

        public LoginRequest( string email, string password, string fingerprint, string useragent )
        {
            Email = email;
            Password = password;
            Fingerprint = fingerprint;
            UserAgent = useragent;
        }
    }
}
