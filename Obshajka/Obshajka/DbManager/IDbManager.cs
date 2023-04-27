using Obshajka.Models;
using Obshajka.Postgres;
using Obshajka.DbManager.Exceptions;
using Obshajka.Postgres.Models;
using Obshajka.Interfaces;

namespace Obshajka.DbManager
{
    public interface IDbManager
    {
        public IEnumerable<IAdvertisement> GetOutsideAdvertisements(int dormitoryId, long userId);
        public IEnumerable<IAdvertisement> GetUserAdvertisements(long userId);
        public void DeleteAdvertisement(long advertId);
        public void AddAdvertisement(Postgres.Models.Advertisement advert);
        public long GetUserIdByEmailAndPassword(string email, string password);
        public long SaveNewUserToDbAndGetId(NewUser newUser);

        public bool CheckUserExist(string email);
    }
}
