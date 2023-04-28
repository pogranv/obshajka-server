using System;
namespace Obshajka.Interfaces
{
	public interface IUser
	{
        public string Name { get; }

        public string Email { get; }

        public string Password { get; }
    }
}

