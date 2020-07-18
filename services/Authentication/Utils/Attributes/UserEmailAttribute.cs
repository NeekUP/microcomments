using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Authentication.Utils.Attributes
{
    public class UserEmailAttribute : DataTypeAttribute
    {
        private static Regex _regex = new Regex( "^[\\w0-9_\\+-]+(\\.[\\w0-9_\\+-]+)*@[\\w0-9-]+(\\.[\\w0-9]+)*\\.([\\w]{2,20})$", RegexOptions.Compiled | RegexOptions.Singleline );

        public UserEmailAttribute()
            : base( DataType.EmailAddress )
        {
            ErrorMessage = "Email address is invalid";
        }

        public override bool IsValid( object value )
        {
            if ( value == null )
                return true;

            var valueAsString = value as string;
            return valueAsString != null && _regex.Match( valueAsString ).Length > 0;
        }
    }
}
