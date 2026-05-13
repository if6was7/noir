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
		public User? CurrentUser { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return RedirectToPage("/Log_In");

			CurrentUser = await _context.Users.FindAsync(userId.Value);

			if (CurrentUser == null)
			{
				HttpContext.Session.Clear();
				return RedirectToPage("/Log_In");
			}

			if (CurrentUser.Role != "admin" && CurrentUser.Role != "superadmin")
				return RedirectToPage("/Index");

			IsSuperAdmin = CurrentUser.Role == "superadmin";

			var all = await _context.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
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

			TempData["Message"] = targetUser.Username + (targetUser.Role == "admin" ? " promoted to admin" : " demoted to user");
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

			// Нельзя удалить самого себя
			if (targetUser.Id == currentUserId.Value)
			{
				TempData["Message"] = "You cannot delete yourself";
				return RedirectToPage();
			}

			// ← КАСКАДНОЕ УДАЛЕНИЕ ВСЕХ СВЯЗЕЙ

			// 1. Listings — обнуляем SellerId (не удаляем сами листинги, пусть висят как "orphan")
			var listings = await _context.Listings.Where(l => l.SellerId == userId).ToListAsync();
			foreach (var l in listings)
			{
				l.SellerId = 0;
				l.Status = "removed";
			}

			// 2. Bids — удаляем
			var bids = await _context.Bids.Where(b => b.UserId == userId).ToListAsync();
			_context.Bids.RemoveRange(bids);

			// 3. Purchases — удаляем
			var purchases = await _context.Purchases.Where(p => p.BuyerId == userId).ToListAsync();
			_context.Purchases.RemoveRange(purchases);

			// 4. Reviews — удаляем
			var reviews = await _context.Reviews.Where(r => r.ReviewerId == userId).ToListAsync();
			_context.Reviews.RemoveRange(reviews);

			// 5. Likes — удаляем
			var likes = await _context.UserLikes.Where(ul => ul.UserId == userId).ToListAsync();
			_context.UserLikes.RemoveRange(likes);

			// 6. Saves — удаляем
			var saves = await _context.UserSaves.Where(us => us.UserId == userId).ToListAsync();
			_context.UserSaves.RemoveRange(saves);

			// 7. SavedCards — удаляем
			var cards = await _context.SavedCards.Where(c => c.UserId == userId).ToListAsync();
			_context.SavedCards.RemoveRange(cards);

			// 8. Subscription — удаляем
			var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId);
			if (sub != null) _context.Subscriptions.Remove(sub);

			// 9. Наконец удаляем самого юзера
			_context.Users.Remove(targetUser);
			await _context.SaveChangesAsync();

			TempData["Message"] = targetUser.Username + " deleted";
			return RedirectToPage();
		}
	}
}