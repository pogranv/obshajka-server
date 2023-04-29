using Obshajka.Models;

namespace Obshajka.Interfaces
{
    public interface IDbManager
    {
        public IEnumerable<Advertisement> GetOutsideAdvertisements(int dormitoryId, long userId);

        public IEnumerable<Advertisement> GetUserAdvertisements(long userId);

        public void DeleteAdvertisement(long advertId);

        public void AddAdvertisement(NewAdvertisement advert);

        public long GetUserIdByEmailAndPassword(string email, string password);

        public long SaveNewUserToDbAndGetId(User newUser);

        public bool CheckUserExist(string email);
    }
}
