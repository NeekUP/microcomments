using Authentication.Domain;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Authentication.Infrastructure
{
    public class HashProvider : IHashProvider
    {
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        public byte[] Hash( byte[] data, byte[] salt )
        {
            using ( SHA256 sha = SHA256.Create() )
            {
                var passBit = new BitArray( data );
                var saltBit = new BitArray( salt );
                var xor = passBit.Xor( saltBit );

                var bytes = new byte[xor.Length / 8];
                xor.CopyTo( bytes, 0 );
                return sha.ComputeHash( bytes );
            }
        }

        public byte[] Hash( string data, byte[] salt )
        {
            var bytes = Encoding.UTF8.GetBytes( data );
            return Hash( bytes, salt );
        }

        public (byte[] hash, byte[] salt) HashPassword( string password )
        {
            var passByte = Encoding.UTF8.GetBytes( password );
            var saltByte = new byte[passByte.Length];
            rngCsp.GetBytes( saltByte );

            var h = Hash( passByte, saltByte );

            return (h, saltByte);
        }
    }
}
