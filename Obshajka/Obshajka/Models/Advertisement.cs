using Obshajka.Interfaces;

namespace Obshajka.Models
{
    public class Advertisement : IAdvertisement
    {
        public long Id { get; set; }

        public string CreatorName { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int DormitoryId { get; set; }

        public int? Price { get; set; }

        public string? Image { get; set; }

        public string DateOfAddition { get; set; }

        public static IEnumerable<Advertisement> BuildAdvertisements(IEnumerable<Postgres.Models.Advertisement> advertisements)
        {
            List<Advertisement> result = new();
            foreach (var advertisement in advertisements)
            {
                result.Add(new Advertisement
                {
                    Id = advertisement.Id,
                    // CreatorName = advertisement.CreatorName, TODO
                    Title = advertisement.Title,
                    Description = advertisement.Description,
                    DormitoryId = advertisement.DormitoryId,
                    Price = advertisement.Price,
                    Image = advertisement.Image,
                    DateOfAddition = advertisement.DateOfAddition.ToString("d"),
                });
            }
            return result;
        }
    }
}
