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
		public bool IsSuperAdmin { get; set; }
		public User? CurrentUser { get; set; }          // ← ДОБАВЛЕНО

		public async Task<IActionResult> OnGetAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return RedirectToPage("/Log_In");

			CurrentUser = await _context.Users.FindAsync(userId.Value); // ← ДОБАВЛЕНО

			var currentUser = await _context.Users.FindAsync(userId.Value);
			if (currentUser == null || (currentUser.Role != "admin" && currentUser.Role != "superadmin"))
				return RedirectToPage("/Index");

			IsSuperAdmin = currentUser.Role == "superadmin";

			var all = await _context.Users.OrderBy(u => u.Id).ToListAsync();
			AdminUsers = all.Where(u => u.Role == "admin" || u.Role == "superadmin").ToList();
			AllUsers = all.Where(u => u.Role == "user").ToList();

			return Page();
		}

		public async Task<IActionResult> OnPostToggleAdminAsync(int userId)
		{
			var currentUserId = HttpContext.Session.GetInt32("UserId");
			if (!currentUserId.HasValue) return Unauthorized();

			var currentUser = await _context.Users.FindAsync(currentUserId.Value);
			if (currentUser == null || currentUser.Role != "superadmin")
				return Unauthorized();

			var targetUser = await _context.Users.FindAsync(userId);
			if (targetUser == null || targetUser.Role == "superadmin")
				return BadRequest();

			targetUser.Role = targetUser.Role == "admin" ? "user" : "admin";
			await _context.SaveChangesAsync();

			return RedirectToPage();
		}

		public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
		{
			var currentUserId = HttpContext.Session.GetInt32("UserId");
			if (!currentUserId.HasValue) return Unauthorized();

			var currentUser = await _context.Users.FindAsync(currentUserId.Value);
			if (currentUser == null || currentUser.Role != "superadmin")
				return Unauthorized();

			var targetUser = await _context.Users.FindAsync(userId);
			if (targetUser == null || targetUser.Role == "superadmin")
				return BadRequest();

			_context.Users.Remove(targetUser);
			await _context.SaveChangesAsync();

			return RedirectToPage();
		}
	}
}