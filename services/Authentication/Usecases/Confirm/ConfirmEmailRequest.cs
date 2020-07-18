using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Usecases
{
    public class ConfirmEmailRequest : Validatable
    {
        [Required]
        public Guid Id { get; private set; }
        [Required]
        public Guid Secret { get; private set; }

        public ConfirmEmailRequest( Guid id, Guid secret )
        {
            Id = id;
            Secret = secret;
        }
    }
}
