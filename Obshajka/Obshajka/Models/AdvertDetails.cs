namespace Obshajka.Models
{
    public class AdvertDetails
    {
        /// <summary>
        /// id владельца объявления
        /// </summary>
        public long CreatorId { get; set; }

        /// <summary>
        /// Заголовок объявления
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Описание объявления
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// id общежития, в котором нужно разместить объявление
        /// </summary>
        public int DormitoryId { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        public int? Price { get; set; }
    }
}
