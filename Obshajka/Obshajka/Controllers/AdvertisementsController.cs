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
        private static readonly ICloudImageStorage _cloudImageStorage;
        private static readonly IDbManager _dbManager;
        static AdvertisementsController()
        {
            _cloudImageStorage = new YandexDisk.YandexDisk();
            _dbManager = new DbManager.DbManager();
        }

        [HttpGet("outsides/{dormitoryId:int:min(1)}/{userId:long:min(1)}")]
        public IActionResult GetOutsideAdvertisements(int dormitoryId, long userId)
        {
            var adverts = _dbManager.GetOutsideAdvertisements(dormitoryId, userId).ToList();
            return Ok(adverts);
        }

        [HttpGet("my/{userId:long:min(1)}")]
        public IActionResult GetUserAdvertisements(long userId)
        {
            var adverts = _dbManager.GetUserAdvertisements(userId);
            return Ok(adverts);
        }

        [HttpDelete("remove/{advertisementId}")]
        public IActionResult DeleteAdvertisement(int advertisementId)
        {
            try
            {
                _dbManager.DeleteAdvertisement(advertisementId);
                return Ok();
            }
            // TODO: на клиенте поддержать исключение
            catch (AdvertisementNotFoundException)
            {
                return NotFound();
            }
            
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddAdvertisement([FromForm] AdvertisementFromFront advert)
        {
            var newAdvertisement = new Postgres.Models.Advertisement();

            newAdvertisement.CreatorId = advert.Details.CreatorId;
            newAdvertisement.Title = advert.Details.Title;
            newAdvertisement.Description = advert.Details.Description;
            newAdvertisement.DormitoryId = advert.Details.DormitoryId;
            newAdvertisement.Price = advert.Details.Price;
            if (advert.Image != null)
            {
                var imageLink = await _cloudImageStorage.UploadImageAndGetLink(advert.Image);
                newAdvertisement.Image = imageLink;
            }
            newAdvertisement.DateOfAddition = DateOnly.FromDateTime(DateTime.Now);
            _dbManager.AddAdvertisement(newAdvertisement);
            return Ok();
        }
    }
}
