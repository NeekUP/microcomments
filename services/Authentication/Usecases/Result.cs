namespace Authentication.Usecases
{
    public class Result
    {
        public bool Success { get; }
        public Error? Error { get; }

        protected Result( Error error ) : this( false )
        {
            Error = error;
        }

        protected Result( bool success )
        {
            Success = success;
        }

        public static Result Fail( Error error )
        {
            return new Result( error );
        }

        public static Result Ok()
        {
            return new Result( true );
        }
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        private Result( T value ) : base( true )
        {
            Value = value;
        }

        private Result( Error error ) : base( error )
        {

        }

        public new static Result<T> Fail( Error error )
        {
            return new Result<T>( error );
        }

        public static Result<T> Ok( T value )
        {
            return new Result<T>( value );
        }
    }
}
