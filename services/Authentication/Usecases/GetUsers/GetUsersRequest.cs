using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Usecases
{
    public class GetUsersRequest : Validatable
    {
        [Required]
        [MinLength( 1 )]
        public Guid[] Ids;
    }
}
