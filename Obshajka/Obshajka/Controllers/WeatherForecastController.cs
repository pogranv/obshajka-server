using Microsoft.AspNetCore.Mvc;
using Obshajka.Postgres;
using Obshajka.Postgres.Models;

namespace Obshajka.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public string Get()
        {
            using (ObshajkaDbContext db = new ObshajkaDbContext())
            {
                var ad = db.Advertisements.FirstOrDefault();
                //var u = ad.Creator;
                //return u.Name;
                db.Entry<Advertisement>(ad).Reference("Creator").Load();
                return ad.Creator.Name;
            }
        }
    }
}