using System;
using Obshajka.Postgres;

namespace Obshajka.DbManager.Models
{
	public class Advertisement
	{
        public long Id { get; set; }

        public string CreatorName { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public int DormitoryId { get; set; }

        public int? Price { get; set; }

        public string? Image { get; set; }

        public string DateOfAddition { get; set; }
    }
}

