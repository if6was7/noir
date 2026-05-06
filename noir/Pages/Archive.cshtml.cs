using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace noir.Pages
{
	public class ArchiveModel : PageModel
	{
		// Модель данных для архивного товара
		public class SoldItem
		{
			public int Id { get; set; }
			public string Title { get; set; }
			public int Price { get; set; }
			public string Date { get; set; }
		}

		public List<SoldItem> SoldItems { get; set; }

		public void OnGet()
		{
			// Имитация базы данных проданных товаров
			SoldItems = new List<SoldItem>
			{
				new SoldItem { Id = 50, Title = "ancient obsidian mask", Price = 12500, Date = "Sold Oct 2025" },
				new SoldItem { Id = 51, Title = "fragmented marble torso", Price = 42000, Date = "Sold Dec 2025" },
				new SoldItem { Id = 52, Title = "silver occult medallion", Price = 3100, Date = "Sold Jan 2026" },
				new SoldItem { Id = 53, Title = "first edition 'the void'", Price = 8900, Date = "Sold Feb 2026" },
				new SoldItem { Id = 54, Title = "rusted knight gauntlet", Price = 15600, Date = "Sold Mar 2026" }
			};
		}
	}
}