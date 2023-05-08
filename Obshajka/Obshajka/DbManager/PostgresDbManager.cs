using Microsoft.EntityFrameworkCore;

using Obshajka.Interfaces;
using Obshajka.YandexDiskApi;

using Obshajka.DbManager.Postgres;
using Obshajka.DbManager.Exceptions;

namespace Obshajka.DbManager
{
    public class PostgresDbManager : IDbManager
    {
        private static readonly ICloudImageStorage s_cloudImageStorage;

        private readonly ILogger<PostgresDbManager> _logger;

        static PostgresDbManager()
        {
            s_cloudImageStorage = new YandexDisk.YandexDisk();
        }

        public PostgresDbManager()
        {
            _logger = LoggerFactory.Create(options => options.AddConsole()).CreateLogger<PostgresDbManager>();
        }

        /// <summary>
        /// Метод возвращает из базы данных объявления в рамках
        /// заданного общежития, но владельцем которых
        /// не является заданный пользователь.
        /// </summary>
        /// <param name="dormitoryId">id общежития</param>
        /// <param name="userId">id пользователя</param>
        /// <returns></returns>
        public IEnumerable<Models.Advertisement> GetOutsideAdvertisements(int dormitoryId, long userId)
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var dbAdverts = db.Advertisements.Where(advert => advert.DormitoryId == dormitoryId && advert.CreatorId != userId).Include(ad => ad.Creator);
                return GetAdvertsFromPgModels(dbAdverts);
            }
        }

        /// <summary>
        /// Метод возвращает из базы данных объявления, владельцем которых
        /// является заданный пользователь.
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
        public IEnumerable<Models.Advertisement> GetUserAdvertisements(long userId)
        {
            if (!CheckUserExist(userId))
            {
                throw new UserNotFoundException();
            }
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var dbAdverts = db.Advertisements.Where(advert => advert.CreatorId == userId).Include(ad => ad.Creator);
                return GetAdvertsFromPgModels(dbAdverts);
            }
        }

        /// <summary>
        /// Метод удаляет из базы данных объявление по заданному идентификатору.
        /// </summary>
        /// <param name="advertId">id объявления</param>
        /// <exception cref="AdvertisementNotFoundException"></exception>
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

        /// <summary>
        /// Метод строит класс объявления для базы данных.
        /// </summary>
        /// <param name="newAdvert">Объявление</param>
        /// <returns></returns>
        private static async Task<Postgres.Models.Advertisement> BuildPgAdvertFromNewAdvert(Models.NewAdvertisement newAdvert)
        {
            var pgAdvert = new Postgres.Models.Advertisement
            {
                CreatorId = newAdvert.CreatorId,
                Title = newAdvert.Title,
                Description = newAdvert.Description,
                DormitoryId = newAdvert.DormitoryId,
                Price = newAdvert.Price,
                DateOfAddition = DateOnly.FromDateTime(DateTime.Now),
            };

            if (newAdvert.Image != null)
            {
               pgAdvert.Image = await s_cloudImageStorage.UploadImageAndGetLink(newAdvert.Image);
            }

            return pgAdvert;
        }

        /// <summary>
        /// Метод добавляет новое объявление в базу данных.
        /// </summary>
        /// <param name="newAdvert">Объявление</param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
        public async Task AddAdvertisement(Models.NewAdvertisement newAdvert)
        {
            if (!CheckUserExist(newAdvert.CreatorId))
            {
                _logger.LogWarning($"Владельца объявления с id={newAdvert.CreatorId} нет в базе данных, объявление не было добавлено");
                throw new UserNotFoundException();
            }

            var pgAdvert = await BuildPgAdvertFromNewAdvert(newAdvert);
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                db.Advertisements.Add(pgAdvert);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Метод возвращает id пользователя из базы данных по почте и паролю.
        /// </summary>
        /// <param name="email">Почта</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
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

        /// <summary>
        /// Метод сохраняет в базу данных пользователя и возвращает его идентификатор.
        /// </summary>
        /// <param name="newUser">Пользователь</param>
        /// <returns></returns>
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

        /// <summary>
        /// Метод проверяет существование пользователя в базе данных по почте.
        /// </summary>
        /// <param name="email">Почта</param>
        /// <returns></returns>
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

        /// <summary>
        /// Метод проверяет существование пользователя в базе данных по id.
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <returns></returns>
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
                CreatorName = pgAdvert.Creator.Name,
                Title = pgAdvert.Title,
                Description = pgAdvert.Description,
                DormitoryId = pgAdvert.DormitoryId,
                Price = pgAdvert.Price,
                Image = pgAdvert.Image,
                DateOfAddition = pgAdvert.DateOfAddition.ToString("d"),
            };
        }

        private IEnumerable<Models.Advertisement> GetAdvertsFromPgModels(IEnumerable<Postgres.Models.Advertisement> pgAdverts)
        {
            List<Models.Advertisement> result = new();
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                foreach (var pgAdvert in pgAdverts)
                {
                    result.Add(GetAdvertFromPgModel(pgAdvert));
                }
                return result;
            }
        }

    }
}
