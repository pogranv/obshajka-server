using Microsoft.AspNetCore.Mvc;

using Obshajka.DbManager;
using Obshajka.Interfaces;
using Obshajka.Models;
using Obshajka.DbManager.Exceptions;

namespace Obshajka.Controllers
{
    [Route("api/v1/adverts")]
    [ApiController]
    public class AdvertisementsController : ControllerBase
    {
        private static readonly IDbManager _postgresDbManager;

        private readonly ILogger<AdvertisementsController> _logger;

        static AdvertisementsController()
        {
            _postgresDbManager = new PostgresDbManager();
        }

        public AdvertisementsController()
        {
            _logger = LoggerFactory.Create(options => options.AddConsole()).CreateLogger<AdvertisementsController>();
        }

        /// <summary>
        /// Метод возвращает объявления в рамках
        /// заданного общежития, но владельцем которых
        /// не является заданный пользователь.
        /// </summary>
        /// <param name="dormitoryId">id общежития</param>
        /// <param name="userId">id пользователя</param>
        /// <returns>Перечисление объявлений</returns>
        [HttpGet("outsides/{dormitoryId:int:min(1)}/{userId:long:min(1)}")]
        public IActionResult GetOutsideAdvertisements(int dormitoryId, long userId)
        {
            try
            {
                var adverts = _postgresDbManager.GetOutsideAdvertisements(dormitoryId, userId).ToList();
                if (adverts.Count == 0)
                {
                    _logger.LogWarning($"По запросу dormitoryId={dormitoryId}, userId={userId} объявлений не найдено");
                }
                return Ok(adverts);
            }
            catch (UserNotFoundException)
            {
                return BadRequest("Указан неверный пользователь");
            }
        }

        /// <summary>
        /// Метод возвращает объявления, владельцем которых
        /// является заданный пользователь.
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <returns></returns>
        [HttpGet("my/{userId:long:min(1)}")]
        public IActionResult GetUserAdvertisements(long userId)
        {
            try
            {
                var adverts = _postgresDbManager.GetUserAdvertisements(userId).ToList();
                if (adverts.Count == 0)
                {
                    _logger.LogWarning($"По запросу userId={userId} объявлений не найдено");
                }
                return Ok(adverts);
            }
            catch (UserNotFoundException)
            {
                return BadRequest("Указан неверный пользователь");
            }
        }

        /// <summary>
        /// Метод удаляет объявление по заданному идентификатору.
        /// </summary>
        /// <param name="advertisementId">id объявления</param>
        /// <returns></returns>
        [HttpDelete("remove/{advertisementId}")]
        public IActionResult DeleteAdvertisement(int advertisementId)
        {
            try
            {
                _postgresDbManager.DeleteAdvertisement(advertisementId);
                return Ok();
            }
            catch (AdvertisementNotFoundException)
            {
                _logger.LogWarning($"Не удалось удалить объявление с id={advertisementId}");
                return NotFound();
            }

        }

        /// <summary>
        /// Метод добавляет новое объявление.
        /// </summary>
        /// <param name="advert">Объявление, которое нужно добавить</param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddAdvertisement([FromForm] AdvertisementFromFront advert)
        {
            try
            {
                await _postgresDbManager.AddAdvertisement(advert.BuildNewAdvertisement());
                return Ok();
            }
            catch (UserNotFoundException)
            {
                _logger.LogWarning($"Не удалось добавить объявление: пользователь с id={advert.Details.CreatorId} не зарегистрирован");
                return BadRequest();
            }
        }
    }
}
