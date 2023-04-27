using Obshajka.Models;
using Obshajka.Postgres;
using Obshajka.DbManager.Exceptions;
using Obshajka.Postgres.Models;
using Obshajka.Interfaces;

namespace Obshajka.DbManager
{
    public class DbManager : IDbManager
    {

        // TODO: эксепшины кидать
        public IEnumerable<IAdvertisement> GetOutsideAdvertisements(int dormitoryId, long userId)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var db_adverts = db.Advertisements.Where(advert => advert.DormitoryId == dormitoryId && advert.CreatorId != userId);
                return Models.Advertisement.BuildAdvertisements(db_adverts);
            }
        }

        public IEnumerable<IAdvertisement> GetUserAdvertisements(long userId)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var db_adverts = db.Advertisements.Where(advert => advert.CreatorId == userId);
                return Models.Advertisement.BuildAdvertisements(db_adverts);
            }
        }

        public void DeleteAdvertisement(long advertId)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                Postgres.Models.Advertisement? advert = db.Advertisements.FirstOrDefault(advert => advert.Id == advertId);
                if (advert == null)
                {
                    throw new AdvertisementNotFoundException();
                }
                db.Advertisements.Remove(advert);
                db.SaveChanges();
            }
        }

        public void AddAdvertisement(Postgres.Models.Advertisement advert)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                // TODO: проверить, корректно ил добавляются имя и id
                db.Advertisements.Add(advert);
                db.SaveChanges();
            }
        }

        public long GetUserIdByEmailAndPassword(string email, string password)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var user = db.Users.FirstOrDefault(user => user.Email == email && user.Password == password);
                if (user != null)
                {
                    return user.Id;
                }
                throw new UserNotFoundException();
            }
        }

        public long SaveNewUserToDbAndGetId(NewUser newUser)
        {
            var user = new User
            {
                Name = newUser.Name,
                Email = newUser.Email,
                Password = newUser.Password
            };
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                db.Users.Add(user);
                db.SaveChanges();
            }
            return user.Id;
        }

        public bool CheckUserExist(string email)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var user = db.Users.FirstOrDefault(user => user.Email == email);
                if (user != null)
                {
                    return true;
                }
                return false;
            }
        }

    }
}
