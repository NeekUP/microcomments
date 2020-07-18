using Authentication.Usecases;
using DnsClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Infrastructure
{
    public class DnsLookup : IDnsLookup
    {
        private static LookupClient _client = new LookupClient();

        public async Task<IEnumerable<string>> QueryMX( string host )
        {
            return (await _client.QueryAsync( host, QueryType.MX )).Answers?
                .Select( a => a.DomainName.Value );
        }
    }
}
