using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using noir.Models;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class TopUpModel : PageModel
	{
		private readonly NoirDbContext _context;

		public TopUpModel(NoirDbContext context)
		{
			_context = context;
		}

		public User? CurrentUser { get; set; }
		public decimal CurrentBalance { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return RedirectToPage("/Log_In");

			CurrentUser = await _context.Users.FindAsync(userId.Value);
			if (CurrentUser == null) return RedirectToPage("/Log_In");

			CurrentBalance = CurrentUser.Balance;
			return Page();
		}
	}
}