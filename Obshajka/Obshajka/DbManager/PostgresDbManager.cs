using Obshajka.DbManager.Postgres;
using Obshajka.DbManager.Exceptions;
using Obshajka.DbManager.Postgres.Models;
using Obshajka.Interfaces;

using Obshajka.YandexDiskApi;

namespace Obshajka.DbManager
{
    public class PostgresDbManager : IDbManager
    {
        private static readonly ICloudImageStorage _cloudImageStorage;

        private readonly ILogger<PostgresDbManager> _logger;

        static PostgresDbManager()
        {
            _cloudImageStorage = new YandexDisk.YandexDisk();
        }

        public PostgresDbManager()
        {
            _logger = LoggerFactory.Create(options => options.AddConsole()).CreateLogger<PostgresDbManager>();
        }

        public IEnumerable<Obshajka.Models.Advertisement> GetOutsideAdvertisements(int dormitoryId, long userId)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var dbAdverts = db.Advertisements.Where(advert => advert.DormitoryId == dormitoryId && advert.CreatorId != userId).ToList();
                return GetAdvertsFromPgModels(dbAdverts);
            }
        }

        public IEnumerable<Obshajka.Models.Advertisement> GetUserAdvertisements(long userId)
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
                    _logger.LogWarning($"Обявления с id={advertId} нет в базе данных, объявление не было удалено");
                    throw new AdvertisementNotFoundException();
                }
                db.Advertisements.Remove(advert);
                db.SaveChanges();
            }
        }

        private static async Task<Postgres.Models.Advertisement> BuildPgAdvertFromNewAdvert(Models.NewAdvertisement newAdvert)
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

        public async void AddAdvertisement(Models.NewAdvertisement newAdvert)
        {
            if (!CheckUserExist(newAdvert.CreatorId))
            {
                _logger.LogWarning($"Владельца объявления с id={newAdvert.CreatorId} нет в базе данных, объявление не было добавлено");
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

        private Models.Advertisement GetAdvertFromPgModel(Postgres.Models.Advertisement pgAdvert)
        {
            return new Models.Advertisement
            {
                Id = pgAdvert.Id,
                CreatorName = pgAdvert.Creator.Name, // TODO: сделать загрузку имени но вроде норм
                Title = pgAdvert.Title,
                Description = pgAdvert.Description,
                DormitoryId = pgAdvert.DormitoryId,
                Price = pgAdvert.Price,
                Image = pgAdvert.Image,
                DateOfAddition = pgAdvert.DateOfAddition.ToString(),
            };
        }

        private IEnumerable<Models.Advertisement> GetAdvertsFromPgModels(IEnumerable<Postgres.Models.Advertisement> pgAdverts)
        {
            List<Models.Advertisement> result = new();
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

    }
}
