using Obshajka.VerificationCodeSender.Interfaces;

namespace Obshajka.Models
{
    public sealed class NewUser : IUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }
}
