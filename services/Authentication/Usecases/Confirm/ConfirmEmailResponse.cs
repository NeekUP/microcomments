namespace Authentication.Usecases
{
    public class ConfirmEmailResponse
    {
        public bool Confirmed { get; }

        public ConfirmEmailResponse( bool confirmed )
        {
            Confirmed = confirmed;
        }
    }
}
