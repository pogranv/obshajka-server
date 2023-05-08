using System;

using Obshajka.Interfaces;

namespace Obshajka.Models
{
    public class User : IUser
    {
        private const string _domainHse = "hse.ru";
        private const string _domainEduHse = "edu.hse.ru";
        private const bool _domainHseEnabled = true;
        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public static User Build(IUser user)
        {
            return new User
            {
                Email = user.Email,
                Password = user.Password,
                Name = user.Name,
            };
        }
        
        public bool IsValidEmail()
        {
            if (string.IsNullOrEmpty(Email))
            {
                return false;
            }
            var loginAndDomain = Email.Trim().Split('@');
            if (loginAndDomain.Length != 2)
            {
                return false;
            }
            if (!(loginAndDomain[0].Length >= 4 && loginAndDomain[0].Length <= 20))
            {
                return false;
            }

            if (_domainHseEnabled)
            {
                return loginAndDomain[1] == _domainHse || loginAndDomain[1] == _domainEduHse;
            }
            return loginAndDomain[1] == _domainEduHse;
        }
    }
}

