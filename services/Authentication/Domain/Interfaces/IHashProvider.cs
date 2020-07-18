namespace Authentication.Domain
{
    public interface IHashProvider
    {
        byte[] Hash( byte[] data, byte[] salt );
        byte[] Hash( string data, byte[] salt );
        (byte[] hash, byte[] salt) HashPassword( string password );
    }
}