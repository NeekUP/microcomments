using System.ComponentModel.DataAnnotations;

namespace Authentication.Usecases
{
    public abstract class Validatable
    {
        public void Validate()
        {
            var context = new ValidationContext( this );
            Validator.ValidateObject( this, context, validateAllProperties: true );
        }
    }
}
