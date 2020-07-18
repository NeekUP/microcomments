using System;

namespace Authentication.Usecases
{
    public class RegisterResponse
    {
        public Guid Id;
        public string Name;

        public RegisterResponse( Guid id, string name )
        {
            Id = id;
            Name = name;
        }
    }
}