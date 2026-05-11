using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class AdminFunctionModel : PageModel
	{
		private readonly NoirDbContext _context;

		public AdminFunctionModel(NoirDbContext context)
		{
			_context = context;
		}

		public List<User> AllUsers { get; set; } = new();
		public List<User> AdminUsers { get; set; } = new();

		public async Task<IActionResult> OnGetAsync()
		{
			var all = await _context.Users.OrderBy(u => u.Id).ToListAsync();
			AdminUsers = all.Where(u => u.Role == "admin").ToList();
			AllUsers = all.Where(u => u.Role != "admin").ToList();
			return Page();
		}
	}
}
