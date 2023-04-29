using Microsoft.AspNetCore.Http;
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
            _postgresDbManager = new DbManager.PostgresDbManager();
        }

        public AdvertisementsController()
        {
            _logger = LoggerFactory.Create(options => options.AddConsole()).CreateLogger<AdvertisementsController>();
        }

        [HttpGet("outsides/{dormitoryId:int:min(1)}/{userId:long:min(1)}")]
        public IActionResult GetOutsideAdvertisements(int dormitoryId, long userId)
        {
            var adverts = _postgresDbManager.GetOutsideAdvertisements(dormitoryId, userId).ToList();
            if (adverts.Count == 0)
            {
                _logger.LogWarning($"По запросу dormitoryId={dormitoryId}, userId={userId} объявлений не найдено");
            }
            return Ok(adverts);
        }

        [HttpGet("my/{userId:long:min(1)}")]
        public IActionResult GetUserAdvertisements(long userId)
        {
            var adverts = _postgresDbManager.GetUserAdvertisements(userId).ToList();
            if (adverts.Count == 0)
            {
                _logger.LogWarning($"По запросу userId={userId} объявлений не найдено");
            }
            return Ok(adverts);
        }

        [HttpDelete("remove/{advertisementId}")]
        public IActionResult DeleteAdvertisement(int advertisementId)
        {
            try
            {
                _postgresDbManager.DeleteAdvertisement(advertisementId);
                return Ok();
            }
            // TODO: на клиенте поддержать исключение
            catch (AdvertisementNotFoundException)
            {
                _logger.LogWarning($"Не удалось удалить объявление с id={advertisementId}");
                return NotFound();
            }

        }

        // todo: проверить, что нельзя отправить пустые titile и все остальное
        [HttpPost("add")]
        public async Task<IActionResult> AddAdvertisement([FromForm] AdvertisementFromFront advert)
        {
            try
            {
                _postgresDbManager.AddAdvertisement(advert.BuildNewAdvertisement());
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
