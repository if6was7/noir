using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using noir.Models;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class NicknameModel : PageModel
	{
		private readonly NoirDbContext _context;

		public NicknameModel(NoirDbContext context)
		{
			_context = context;
		}

		[BindProperty] public string Nickname { get; set; } = "";

		public async Task<IActionResult> OnGetAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return RedirectToPage("/Log_In");
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return Unauthorized();

			var user = await _context.Users.FindAsync(userId.Value);
			if (user == null) return NotFound();

			if (!string.IsNullOrWhiteSpace(Nickname))
			{
				user.Nickname = Nickname;
				await _context.SaveChangesAsync();
			}

			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
				return new JsonResult(new { success = true, nickname = user.Nickname });

			return RedirectToPage("/Account");
		}
	}
}