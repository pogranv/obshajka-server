using System;
using Microsoft.AspNetCore.Mvc;
using Obshajka.Binders;
using Obshajka.Models;
using Obshajka.Postgres.Models;

namespace Obshajka.DbManager.Models
{
	public class NewAdvertisement
	{
        public long CreatorId { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int DormitoryId { get; set; }

        public int? Price { get; set; }

        public IFormFile Image { get; set; }
    }
}

