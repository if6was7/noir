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
			var username = request.Username.StartsWith("@@") ? request.Username : "@@" + request.Username;

			var user = await _context.Users
				.FirstOrDefaultAsync(u => u.Username == username);

			if (user == null || user.PasswordHash != request.Password)
				return Unauthorized(new { error = "Invalid credentials" });

			HttpContext.Session.SetInt32("UserId", user.Id);

			return Ok(new
			{
				id = user.Id,
				username = user.Username,
				email = user.Email,
				role = user.Role,
				hasPlus = user.HasPlus,
				balance = user.Balance,
				name = user.Username
			});
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		{
			var username = request.Username.StartsWith("@@") ? request.Username : "@@" + request.Username;

			if (await _context.Users.AnyAsync(u => u.Username == username))
				return BadRequest(new { error = "Username already taken" });

			if (await _context.Users.AnyAsync(u => u.Email == request.Email))
				return BadRequest(new { error = "Email already registered" });

			var user = new User
			{
				Username = username,
				Email = request.Email,
				PasswordHash = request.Password,
				Balance = 0,
				HasPlus = false,
				Role = "user"
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			HttpContext.Session.SetInt32("UserId", user.Id);

			return Ok(new
			{
				success = true,
				id = user.Id,
				username = user.Username,
				email = user.Email,
				role = user.Role,
				hasPlus = user.HasPlus,
				balance = user.Balance
			});
		}

		[HttpPost("google")]
		public async Task<IActionResult> GoogleLogin([FromBody] GoogleRequest request)
		{
			try
			{
				var parts = request.Credential.Split('.');
				if (parts.Length != 3)
					return BadRequest(new { error = "Invalid token format" });

				var base64Payload = parts[1]
					.Replace('-', '+')
					.Replace('_', '/');
				var pad = (4 - base64Payload.Length % 4) % 4;
				base64Payload = base64Payload.PadRight(base64Payload.Length + pad, '=');

				var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(base64Payload));
				var payload = JsonSerializer.Deserialize<GooglePayload>(payloadJson);

				if (payload == null || string.IsNullOrEmpty(payload.email))
					return BadRequest(new { error = "Invalid payload" });

				var emailBase = Regex.Replace(payload.email.Split('@')[0], @"[^a-zA-Z0-9_]", "_");
				var username = "@@" + emailBase;

				var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.email);
				if (user == null)
				{
					if (await _context.Users.AnyAsync(u => u.Username == username))
						username = username + "_" + new Random().Next(1000, 9999);

					user = new User
					{
						Username = username,
						Email = payload.email,
						PasswordHash = Guid.NewGuid().ToString(),
						Role = "user",
						Balance = 0,
						HasPlus = false
					};
					_context.Users.Add(user);
					await _context.SaveChangesAsync();
				}

				HttpContext.Session.SetInt32("UserId", user.Id);

				return Ok(new
				{
					id = user.Id,
					username = user.Username,
					email = user.Email,
					name = payload.name,
					avatar = payload.picture,
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

		[HttpGet("user/{username}")]
		public async Task<IActionResult> GetUser(string username)
		{
			var user = await _context.Users
				.Include(u => u.Listings)
				.FirstOrDefaultAsync(u => u.Username == username);

			if (user == null)
				return NotFound(new { error = "User not found" });

			return Ok(new
			{
				username = user.Username,
				email = user.Email,
				role = user.Role,
				hasPlus = user.HasPlus,
				balance = user.Balance,
				listingCount = user.Listings.Count(l => !l.IsRemoved && l.Status != "sold")
			});
		}
	}

	public class LoginRequest
	{
		public string Username { get; set; } = "";
		public string Password { get; set; } = "";
	}

	public class RegisterRequest
	{
		public string Username { get; set; } = "";
		public string Email { get; set; } = "";
		public string Password { get; set; } = "";
	}

	public class GoogleRequest
	{
		public string Credential { get; set; } = "";
	}

	public class GooglePayload
	{
		public string email { get; set; } = "";
		public string name { get; set; } = "";
		public string picture { get; set; } = "";
	}
}