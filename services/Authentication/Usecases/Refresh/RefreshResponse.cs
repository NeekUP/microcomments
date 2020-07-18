namespace Authentication.Usecases
{
    public class RefreshResponse
    {
        public string AuthToken { get; }
        public string RefreshToken { get; }

        public RefreshResponse( string authToken, string refreshToken )
        {
            AuthToken = authToken;
            RefreshToken = refreshToken;
        }
    }
}
