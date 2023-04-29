using Obshajka.Interfaces;

namespace Obshajka.Interfaces
{
    public interface IVerificationCodesManager
    {
        public void AddUser(IUser user);
        public void SendCodeToUser(string userEmail);
        public IUser VerifyUser(string userEmail, string verificationCode);
    }
}
