using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Obshajka.Binders;
using Obshajka.Models;

namespace Obshajka.Models
{
    public class AdvertisementFromFront
    {
        public IFormFile? Image { get; set; }

        //[ModelBinder(BinderType = typeof(FormDataJsonBinder))]
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
