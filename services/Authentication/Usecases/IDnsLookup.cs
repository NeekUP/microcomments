using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authentication.Usecases
{
    public interface IDnsLookup
    {
        Task<IEnumerable<string>> QueryMX( string host );
    }
}