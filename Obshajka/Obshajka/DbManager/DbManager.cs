using Obshajka.Postgres;
using Obshajka.DbManager.Exceptions;
using Obshajka.Postgres.Models;
using Obshajka.YandexDiskApi;
using Obshajka.DbManager.Models;

namespace Obshajka.DbManager
{
    public class PostgresDbManager : IDbManager
    {
        private static readonly ICloudImageStorage _cloudImageStorage;

        static PostgresDbManager()
        {
            _cloudImageStorage = new YandexDisk.YandexDisk();
        }

        private DbManager.Models.Advertisement GetAdvertFromPgModel(Postgres.Models.Advertisement pgAdvert)
        {
            return new DbManager.Models.Advertisement
            {
                Id = pgAdvert.Id,
                CreatorName = pgAdvert.Creator.Name, // TODO: сделать загрузку имени
                Title = pgAdvert.Title,
                Description = pgAdvert.Description,
                DormitoryId = pgAdvert.DormitoryId,
                Price = pgAdvert.Price,
                Image = pgAdvert.Image,
                DateOfAddition = pgAdvert.DateOfAddition.ToString(),
            };
        }

        private IEnumerable<DbManager.Models.Advertisement> GetAdvertsFromPgModels(IEnumerable<Postgres.Models.Advertisement> pgAdverts)
        {
            List<DbManager.Models.Advertisement> result = new();
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                foreach (var pgAdvert in pgAdverts)
                {
                    db.Entry<Postgres.Models.Advertisement>(pgAdvert).Reference("Creator").Load();
                    result.Add(GetAdvertFromPgModel(pgAdvert));
                }
                return result;
            }
        }

        // TODO: эксепшины кидать
        public IEnumerable<DbManager.Models.Advertisement> GetOutsideAdvertisements(int dormitoryId, long userId)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var dbAdverts = db.Advertisements.Where(advert => advert.DormitoryId == dormitoryId && advert.CreatorId != userId).ToList();
                return GetAdvertsFromPgModels(dbAdverts);
            }
        }

        public IEnumerable<DbManager.Models.Advertisement> GetUserAdvertisements(long userId)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var dbAdverts = db.Advertisements.Where(advert => advert.CreatorId == userId);
                return GetAdvertsFromPgModels(dbAdverts);
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

        private async Task<Postgres.Models.Advertisement> BuildPgAdvertFromNewAdvert(NewAdvertisement newAdvert)
        {
            var imageLink = await _cloudImageStorage.UploadImageAndGetLink(newAdvert.Image);
            return new Postgres.Models.Advertisement
            {
                CreatorId = newAdvert.CreatorId,
                Title = newAdvert.Title,
                Description = newAdvert.Description,
                DormitoryId = newAdvert.DormitoryId,
                Price = newAdvert.Price,
                Image = imageLink,
                DateOfAddition = DateOnly.FromDateTime(DateTime.Now),
            };
        }

        public async void AddAdvertisement(NewAdvertisement newAdvert)
        {
            if (!CheckUserExist(newAdvert.CreatorId))
            {
                throw new UserNotFoundException();
            }
            var pgAdvert = await BuildPgAdvertFromNewAdvert(newAdvert);
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                // TODO: проверить, корректно ил добавляются имя и id
                db.Advertisements.Add(pgAdvert);
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

        public long SaveNewUserToDbAndGetId(Models.User newUser)
        {
            var user = new Postgres.Models.User
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

        public bool CheckUserExist(long userId)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var user = db.Users.FirstOrDefault(user => user.Id == userId);
                if (user != null)
                {
                    return true;
                }
                return false;
            }
        }

    }
}
