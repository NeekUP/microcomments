using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public class DuplicateItemExceptions : Exception
    {
        public DuplicateItemExceptions( string message ) : base( message )
        {

        }

        public DuplicateItemExceptions( string message, Exception innerException ) : base( message, innerException )
        {

        }
    }
}
