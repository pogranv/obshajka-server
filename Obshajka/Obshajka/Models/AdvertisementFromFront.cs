using Microsoft.AspNetCore.Mvc;
using Obshajka.Binders;

namespace Obshajka.Models
{
    public class AdvertisementFromFront
    {
        public IFormFile Image { get; set; }

        [ModelBinder(BinderType = typeof(FormDataJsonBinder))]
        public AdvertDetails Details { get; set; }
    }
}
