using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Authentication.Domain
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string NormalizedEmail { get; private set; }
        public bool EmailConfirmed { get; private set; }
        public string EmailConfirmationSecret { get; set; }
        public byte[] PasswordHash { get; private set; }
        public byte[] Salt { get; private set; }

        protected User() { }

        public User( string email, string name, string password, IHashProvider hashProvider )
        {
            if( hashProvider == null)
                throw new ArgumentException( nameof( hashProvider ) );

            string idnPass = ToPunyCode( password );
            var (hash, salt) = hashProvider.HashPassword( idnPass );

            Email = email ?? throw new ArgumentException( nameof( email ) );
            NormalizedEmail = ToPunyCode( email.ToLower() );
            Name = name ?? throw new ArgumentException( nameof( name ) );
            PasswordHash = hash;
            Salt = salt; 
        }

        protected User( Guid id, string name, string email, bool emailConfirmed, byte[] passwordHash, byte[] salt )
        {
            Id = id;
            Name = name;
            Email = email;
            EmailConfirmed = emailConfirmed;
            PasswordHash = passwordHash;
            Salt = salt;
        }

        public void SetEmailAsConfirmed()
        {
            EmailConfirmationSecret = null;
            EmailConfirmed = true;
        }

        public bool IsValidPassword( string password, IHashProvider hashProvider )
        {
            string idnPass = ToPunyCode( password );
            var hash = hashProvider.Hash( Encoding.UTF8.GetBytes( idnPass ), Salt );

            if ( PasswordHash.Length != hash.Length )
                return false;

            for ( int i = 0; i < PasswordHash.Length; i++ )
            {
                if ( PasswordHash[i] != hash[i] )
                    return false;
            }

            return true;
        }

        private static string ToPunyCode( string str )
        {
            return new IdnMapping().GetAscii( str );
        }
    }
}
