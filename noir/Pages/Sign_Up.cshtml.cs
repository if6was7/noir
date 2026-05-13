using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class Sign_UpModel : PageModel
	{
		private readonly NoirDbContext _context;

		public Sign_UpModel(NoirDbContext context)
		{
			_context = context;
		}

		[BindProperty] public string Username { get; set; } = "";
		[BindProperty] public string Email { get; set; } = "";
		[BindProperty] public string Password { get; set; } = "";

		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (await _context.Users.AnyAsync(u => u.Username == Username))
			{
				ModelState.AddModelError("Username", "Username already taken");
				return Page();
			}

			if (await _context.Users.AnyAsync(u => u.Email == Email))
			{
				ModelState.AddModelError("Email", "Email already registered");
				return Page();
			}

			var user = new User
			{
				Username = Username,
				Email = Email,
				PasswordHash = Password,
				Nickname = Username,
				Balance = 0,
				HasPlus = false,
				Role = "user"
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			HttpContext.Session.SetInt32("UserId", user.Id);
			HttpContext.Session.SetString("UserRole", user.Role);

			return RedirectToPage("/Index");
		}
	}
}