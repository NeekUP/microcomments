using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Usecases
{
    public class GetUserRequest : Validatable
    {
        [Required]
        public Guid Id { get; }

        public GetUserRequest( Guid id )
        {
            Id = id;
        }
    }
}
