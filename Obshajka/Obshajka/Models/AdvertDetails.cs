namespace Obshajka.Models
{
    public class AdvertDetails
    {
        public long CreatorId { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int DormitoryId { get; set; }

        public int? Price { get; set; }
    }
}
