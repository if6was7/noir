using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace noir.Pages
{
	public class UploadModel : PageModel
	{
		public int ActiveAdsCount { get; set; } = 2;
		public int MaxFreeAds { get; set; } = 3;
		public bool IsLimitReached => ActiveAdsCount >= MaxFreeAds;

		[BindProperty]
		public ItemUploadInput Input { get; set; } = new(); // ← важно

		public void OnGet() { }

		public IActionResult OnPost()
		{
			if (!ModelState.IsValid) return Page();

			if (IsLimitReached)
			{
				ModelState.AddModelError(string.Empty, "Limit reached.");
				return Page();
			}

			// 👇 серверная подстраховка (если MinBid не ввели)
			if (Input.IsAuction && (!Input.MinBid.HasValue || Input.MinBid == 0))
			{
				Input.MinBid = (int)Math.Round(Input.Price * 0.1);
			}

			// TODO: сохранить в БД

			return RedirectToPage("/Index");
		}

		public class ItemUploadInput
		{
			[Required]
			public string Title { get; set; } = "";

			[Required]
			public int Price { get; set; }

			public string Description { get; set; } = "";
			public string Categories { get; set; } = "";

			public bool IsAuction { get; set; }

			public int? MinBid { get; set; } // ← ВОТ ОНО
		}
	}
}