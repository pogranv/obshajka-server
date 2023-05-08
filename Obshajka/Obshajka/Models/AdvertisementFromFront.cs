using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Obshajka.Models;

namespace Obshajka.Models
{
    public class AdvertisementFromFront
    {
        /// <summary>
        /// Картинка объявления.
        /// </summary>
        public IFormFile? Image { get; set; }

        /// <summary>
        /// Информация об объявлении.
        /// </summary>
        public AdvertDetails Details { get; set; }

        public NewAdvertisement BuildNewAdvertisement()
        {
            return new NewAdvertisement
            {
                CreatorId = Details.CreatorId,
                Title = Details.Title,
                Description = Details.Description,
                DormitoryId = Details.DormitoryId,
                Price = Details.Price,
                Image = Image,
            };
        }
    }
}
