using Microsoft.AspNetCore.Mvc;
using Obshajka.Binders;
using Obshajka.DbManager.Models;

namespace Obshajka.Models
{
    public class AdvertisementFromFront
    {
        public IFormFile Image { get; set; }

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
                Image = Image,
            };
        }
    }
}
