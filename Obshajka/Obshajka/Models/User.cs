using System;
using Obshajka.Interfaces;

namespace Obshajka.DbManager.Models
{
    public class User : IUser
    {
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
    }
}

