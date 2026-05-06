using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace noir.Pages
{
	public class IndexModel : PageModel
	{
		public List<AntiqueItem> Items { get; set; } = new();

		public void OnGet()
		{
			Items = new List<AntiqueItem>
			{
				new AntiqueItem { Id = 1, Title = "Leica III (1933) — Special Edition", Price = 18500 },
				new AntiqueItem { Id = 2, Title = "Vacheron Constantin — Pocket Gold", Price = 42000 },
				new AntiqueItem { Id = 3, Title = "S.T. Dupont — Ligne 1 Briquet", Price = 15200 },
				new AntiqueItem { Id = 4, Title = "Original Hermes Kelly (1950s)", Price = 27000 }
			};
		}
	}

	public class AntiqueItem
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public decimal Price { get; set; }
	}
}