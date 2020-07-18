using Authentication.Domain;
using System;

namespace Authentication.Events
{
    public class UserRegistered
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public bool EmailConfirmed { get; private set; }
        public string EmailConfirmationSecret { get; set; }

        public UserRegistered(User user )
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email;
            EmailConfirmed = user.EmailConfirmed;
            EmailConfirmationSecret = user.EmailConfirmationSecret;
        }
    }
}
