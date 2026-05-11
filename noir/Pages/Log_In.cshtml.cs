using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace noir.Pages
{
	public class Log_InModel : PageModel
	{
		public IActionResult OnGet()
		{
			if (HttpContext.Session.GetInt32("UserId").HasValue)
				return RedirectToPage("/Index");
			return Page();
		}
	}
}