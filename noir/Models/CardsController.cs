using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System;
using System.Threading.Tasks;

namespace noir.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CardsController : ControllerBase
	{
		private readonly NoirDbContext _context;

		public CardsController(NoirDbContext context)
		{
			_context = context;
		}

		public class LinkCardRequest
		{
			public string Username { get; set; } = "";
			public string Network { get; set; } = "";
			public string Last4 { get; set; } = "";
			public string Expiry { get; set; } = "";
		}

		[HttpPost]
		public async Task<IActionResult> LinkCard([FromBody] LinkCardRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.Username))
					return BadRequest(new { error = "Username required" });

				var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
				if (user == null)
					return NotFound(new { error = "User not found" });

				var existingCard = await _context.SavedCards
					.FirstOrDefaultAsync(c => c.UserId == user.Id && c.Last4 == request.Last4 && c.Network == request.Network);

				if (existingCard != null)
					return BadRequest(new { error = "This card is already linked" });

				var card = new SavedCard
				{
					UserId = user.Id,
					Network = request.Network,
					Last4 = request.Last4,
					Expiry = request.Expiry
				};

				_context.SavedCards.Add(card);
				await _context.SaveChangesAsync();

				return Ok(new { success = true, cardId = card.Id });
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpDelete("{cardId}")]
		public async Task<IActionResult> DeleteCard(int cardId)
		{
			try
			{
				var userId = HttpContext.Session.GetInt32("UserId");
				if (!userId.HasValue)
					return Unauthorized(new { error = "Not logged in" });

				var card = await _context.SavedCards.FindAsync(cardId);
				if (card == null || card.UserId != userId.Value)
					return NotFound(new { error = "Card not found" });

				_context.SavedCards.Remove(card);
				await _context.SaveChangesAsync();

				return Ok(new { success = true });
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetCards()
		{
			try
			{
				var userId = HttpContext.Session.GetInt32("UserId");
				if (!userId.HasValue)
					return Unauthorized(new { error = "Not logged in" });

				var cards = await _context.SavedCards
					.Where(c => c.UserId == userId.Value)
					.Select(c => new
					{
						id = c.Id,
						network = c.Network,
						last4 = c.Last4,
						expiry = c.Expiry
					})
					.ToListAsync();

				return Ok(cards);
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}
	}
}