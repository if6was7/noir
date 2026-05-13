using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace noir.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly NoirDbContext _context;

		public AuthController(NoirDbContext context)
		{
			_context = context;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			try
			{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

				if (user == null || user.PasswordHash != request.Password)
					return Unauthorized(new { error = "Invalid credentials" });

				HttpContext.Session.SetInt32("UserId", user.Id);
				HttpContext.Session.SetString("UserRole", user.Role);

				return Ok(new
				{
					id = user.Id,
					username = user.Username,
					displayName = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname,
					nickname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname,
					email = user.Email,
					role = user.Role,
					hasPlus = user.HasPlus,
					balance = user.Balance
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		{
			try
			{
				if (await _context.Users.AnyAsync(u => u.Username == request.Username))
					return BadRequest(new { error = "Username already taken" });

				if (await _context.Users.AnyAsync(u => u.Email == request.Email))
					return BadRequest(new { error = "Email already registered" });

				var user = new User
				{
					Username = request.Username,
					Email = request.Email,
					PasswordHash = request.Password,
					Nickname = request.Username,
					Balance = 0,
					HasPlus = false,
					Role = "user"
				};

				_context.Users.Add(user);
				await _context.SaveChangesAsync();

				HttpContext.Session.SetInt32("UserId", user.Id);
				HttpContext.Session.SetString("UserRole", user.Role);

				return Ok(new
				{
					success = true,
					isNew = true,
					id = user.Id,
					username = user.Username,
					displayName = user.Nickname,
					nickname = user.Nickname,
					email = user.Email,
					role = user.Role,
					hasPlus = user.HasPlus,
					balance = user.Balance
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost("google")]
		public async Task<IActionResult> GoogleLogin([FromBody] GoogleRequest request)
		{
			try
			{
				var parts = request.Credential.Split('.');
				if (parts.Length != 3)
					return BadRequest(new { error = "Invalid token" });

				var base64Payload = parts[1].Replace('-', '+').Replace('_', '/');
				var pad = (4 - base64Payload.Length % 4) % 4;
				base64Payload = base64Payload.PadRight(base64Payload.Length + pad, '=');

				var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(base64Payload));
				var payload = JsonSerializer.Deserialize<GooglePayload>(payloadJson);

				if (payload == null || string.IsNullOrEmpty(payload.email))
					return BadRequest(new { error = "Invalid payload" });

				var emailBase = Regex.Replace(payload.email.Split('@')[0], @"[^a-zA-Z0-9_]", "_");
				var username = emailBase;

				bool isNewUser = false;
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.email);

				if (user == null)
				{
					isNewUser = true;
					if (await _context.Users.AnyAsync(u => u.Username == username))
						username = username + "_" + new Random().Next(1000, 9999);

					user = new User
					{
						Username = username,
						Email = payload.email,
						PasswordHash = Guid.NewGuid().ToString(),
						Nickname = username, // user will set their own on /Nickname page
						Role = "user",
						Balance = 0,
						HasPlus = false
					};
					_context.Users.Add(user);
					await _context.SaveChangesAsync();
				}

				HttpContext.Session.SetInt32("UserId", user.Id);
				HttpContext.Session.SetString("UserRole", user.Role);

				return Ok(new
				{
					id = user.Id,
					username = user.Username,
					displayName = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname,
					nickname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname,
					email = user.Email,
					name = payload.name,
					avatar = payload.picture,
					role = user.Role,
					hasPlus = user.HasPlus,
					balance = user.Balance,
					isNew = isNewUser
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost("apple")]
		public async Task<IActionResult> AppleLogin([FromBody] AppleRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
					return BadRequest(new { error = "Username and password required" });

				if (string.IsNullOrEmpty(request.Email))
					return BadRequest(new { error = "Email required" });

				var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
				if (existingUser != null)
				{
					HttpContext.Session.SetInt32("UserId", existingUser.Id);
					HttpContext.Session.SetString("UserRole", existingUser.Role);
					return Ok(new
					{
						success = true,
						isNew = false,
						id = existingUser.Id,
						username = existingUser.Username,
						displayName = string.IsNullOrEmpty(existingUser.Nickname) ? existingUser.Username : existingUser.Nickname,
						nickname = string.IsNullOrEmpty(existingUser.Nickname) ? existingUser.Username : existingUser.Nickname,
						email = existingUser.Email,
						role = existingUser.Role,
						hasPlus = existingUser.HasPlus,
						balance = existingUser.Balance
					});
				}

				if (await _context.Users.AnyAsync(u => u.Username == request.Username))
					return BadRequest(new { error = "Username already taken" });

				if (await _context.Users.AnyAsync(u => u.Email == request.Email))
					return BadRequest(new { error = "Email already registered" });

				var user = new User
				{
					Username = request.Username,
					Email = request.Email,
					PasswordHash = request.Password,
					Nickname = request.Nickname ?? request.Username,
					Role = "user",
					Balance = 0,
					HasPlus = false
				};

				_context.Users.Add(user);
				await _context.SaveChangesAsync();

				HttpContext.Session.SetInt32("UserId", user.Id);
				HttpContext.Session.SetString("UserRole", user.Role);

				return Ok(new
				{
					success = true,
					isNew = true,
					id = user.Id,
					username = user.Username,
					displayName = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname,
					nickname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname,
					email = user.Email,
					role = user.Role,
					hasPlus = user.HasPlus,
					balance = user.Balance
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpPost("logout")]
		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return Ok(new { success = true });
		}

		[HttpGet("me")]
		public async Task<IActionResult> Me()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return Unauthorized(new { error = "Not logged in" });

			var user = await _context.Users.FindAsync(userId.Value);
			if (user == null)
			{
				HttpContext.Session.Clear();
				return Unauthorized(new { error = "User not found" });
			}

			return Ok(new
			{
				id = user.Id,
				username = user.Username,
				displayName = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname,
				nickname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname,
				email = user.Email,
				role = user.Role,
				hasPlus = user.HasPlus,
				balance = user.Balance
			});
		}
	}

	public class LoginRequest { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
	public class RegisterRequest { public string Username { get; set; } = ""; public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
	public class GoogleRequest { public string Credential { get; set; } = ""; }
	public class GooglePayload { public string email { get; set; } = ""; public string name { get; set; } = ""; public string picture { get; set; } = ""; }
	public class AppleRequest { public string Username { get; set; } = ""; public string Password { get; set; } = ""; public string Email { get; set; } = ""; public string Nickname { get; set; } = ""; }
}