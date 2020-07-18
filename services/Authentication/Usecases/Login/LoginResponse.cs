namespace Authentication.Usecases
{
    public class LoginResponse
    {
        public string AuthToken { get; }
        public string RefreshToken { get; }

        public LoginResponse( string authToken, string refreshToken )
        {
            AuthToken = authToken;
            RefreshToken = refreshToken;
        }
    }
}
