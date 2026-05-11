using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class ArchiveModel : PageModel
	{
		private readonly NoirDbContext _context;

		public ArchiveModel(NoirDbContext context)
		{
			_context = context;
		}

		public List<Listing> ArchivedItems { get; set; } = new();

		public async Task OnGetAsync()
		{
			ArchivedItems = await _context.Listings
				.Include(l => l.Seller)
				.Where(l => l.Status == "sold" || l.IsRemoved)
				.OrderBy(l => l.Id)
				.ToListAsync();
		}
	}
}
