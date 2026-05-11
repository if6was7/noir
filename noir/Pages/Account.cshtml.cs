using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Linq;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class AccountModel : PageModel
	{
		private readonly NoirDbContext _context;

		public AccountModel(NoirDbContext context)
		{
			_context = context;
		}

		public User? UserProfile { get; set; }
		public bool IsAdmin { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			// Will be populated by JS from sessionStorage, but we can load cards/subscription from DB
			return Page();
		}
	}
}
