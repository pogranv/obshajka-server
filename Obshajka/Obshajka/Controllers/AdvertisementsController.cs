using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Obshajka.Binders;
using Obshajka.Postgres;
using Obshajka.Postgres.Models;
using Obshajka.Models;
using Obshajka.YandexDiskApi;
using Obshajka.DbManager;
using Obshajka.DbManager.Exceptions;

namespace Obshajka.Controllers
{
    [Route("api/v1/adverts")]
    [ApiController]
    public class AdvertisementsController : ControllerBase
    {
        
        private static readonly IDbManager _postgresDbManager;

        static AdvertisementsController()
        {
            _postgresDbManager = new DbManager.PostgresDbManager();
        }

        [HttpGet("outsides/{dormitoryId:int:min(1)}/{userId:long:min(1)}")]
        public IActionResult GetOutsideAdvertisements(int dormitoryId, long userId)
        {
            var adverts = _postgresDbManager.GetOutsideAdvertisements(dormitoryId, userId).ToList();
            return Ok(adverts);
        }

        [HttpGet("my/{userId:long:min(1)}")]
        public IActionResult GetUserAdvertisements(long userId)
        {
            var adverts = _postgresDbManager.GetUserAdvertisements(userId);
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
                // TODO: jgддержать на фронте
                return BadRequest();
            }
        }
    }
}
