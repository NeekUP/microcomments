using Authentication.Utils.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Usecases
{
    public class RegisterRequest : Validatable
    {
        [Required()]
        [StringLength( maximumLength: 64, MinimumLength = 2 )]
        [RegularExpression( @"^[^\r\n\s]+$" )]
        public string Name { get; }

        [Required()]
        [UserEmail()]
        public string Email { get; }

        [Required()]
        [StringLength( maximumLength: 64, MinimumLength = 6 )]
        [RegularExpression( @"^[^\r\n]+$" )]
        public string Password { get; set; }

        public RegisterRequest( string name, string email, string password )
        {
            Name = name;
            Email = email;
            Password = password;
        }
    }
}
