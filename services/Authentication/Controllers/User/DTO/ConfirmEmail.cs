using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Controllers.User.DTO
{
    public class ConfirmEmailRequest
    {
        [Required]
        public Guid Id;
        [Required]
        public Guid Secret;
    }
}
