using Microsoft.EntityFrameworkCore;
using noir.Models;

namespace noir
{
	public class NoirDbContext : DbContext
	{
		public NoirDbContext(DbContextOptions<NoirDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Listing> Listings { get; set; }
		public DbSet<AuctionLot> AuctionLots { get; set; }
		public DbSet<Bid> Bids { get; set; }
		public DbSet<Purchase> Purchases { get; set; }
		public DbSet<Subscription> Subscriptions { get; set; }
		public DbSet<SavedCard> SavedCards { get; set; }
		public DbSet<Review> Reviews { get; set; }
		public DbSet<UserLike> UserLikes { get; set; }
		public DbSet<UserSave> UserSaves { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// User
			modelBuilder.Entity<User>()
				.HasIndex(u => u.Username)
				.IsUnique();

			modelBuilder.Entity<User>()
				.HasIndex(u => u.Email)
				.IsUnique();

			modelBuilder.Entity<User>()
				.Property(u => u.Balance)
				.HasPrecision(18, 2);

			// Super-admin seed
			modelBuilder.Entity<User>().HasData(new User
			{
				Id = 1,
				Username = "if6was7",
				Email = "super@noir.local",
				PasswordHash = "67890",
				Balance = 999999.99m,
				HasPlus = true,
				Role = "superadmin",
				Nickname = "Super Admin"
			});

			// Listing
			modelBuilder.Entity<Listing>()
				.HasOne(l => l.Seller)
				.WithMany(u => u.Listings)
				.HasForeignKey(l => l.SellerId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Listing>()
				.Property(l => l.Price)
				.HasPrecision(18, 2);

			// AuctionLot -> Listing (one-to-one)
			modelBuilder.Entity<AuctionLot>()
				.HasOne(a => a.Listing)
				.WithOne(l => l.AuctionLot)
				.HasForeignKey<AuctionLot>(a => a.ListingId);

			modelBuilder.Entity<AuctionLot>()
				.Property(a => a.StartPrice)
				.HasPrecision(18, 2);

			modelBuilder.Entity<AuctionLot>()
				.Property(a => a.CurrentPrice)
				.HasPrecision(18, 2);

			// Bid -> User (restrict delete)
			modelBuilder.Entity<Bid>()
				.HasOne(b => b.User)
				.WithMany(u => u.Bids)
				.HasForeignKey(b => b.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Bid>()
				.Property(b => b.Amount)
				.HasPrecision(18, 2);

			// Purchase -> Buyer (restrict delete)
			modelBuilder.Entity<Purchase>()
				.HasOne(p => p.Buyer)
				.WithMany(u => u.Purchases)
				.HasForeignKey(p => p.BuyerId)
				.OnDelete(DeleteBehavior.Restrict);

			// Purchase -> Listing (restrict delete)
			modelBuilder.Entity<Purchase>()
				.HasOne(p => p.Listing)
				.WithMany(l => l.Purchases)
				.HasForeignKey(p => p.ListingId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Purchase>()
				.Property(p => p.Amount)
				.HasPrecision(18, 2);

			// Subscription -> User (one-to-one)
			modelBuilder.Entity<Subscription>()
				.HasOne(s => s.User)
				.WithOne(u => u.Subscription)
				.HasForeignKey<Subscription>(s => s.UserId);

			modelBuilder.Entity<Subscription>()
				.Property(s => s.Price)
				.HasPrecision(18, 2);

			// Review -> Reviewer (restrict delete)
			modelBuilder.Entity<Review>()
				.HasOne(r => r.Reviewer)
				.WithMany(u => u.Reviews)
				.HasForeignKey(r => r.ReviewerId)
				.OnDelete(DeleteBehavior.Restrict);

			// Review -> Listing (restrict delete)
			modelBuilder.Entity<Review>()
				.HasOne(r => r.Listing)
				.WithMany(l => l.Reviews)
				.HasForeignKey(r => r.ListingId)
				.OnDelete(DeleteBehavior.Restrict);

			// UserLike
			modelBuilder.Entity<UserLike>()
				.HasOne(ul => ul.User)
				.WithMany(u => u.Likes)
				.HasForeignKey(ul => ul.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserLike>()
				.HasOne(ul => ul.Listing)
				.WithMany()
				.HasForeignKey(ul => ul.ListingId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserLike>()
				.HasIndex(ul => new { ul.UserId, ul.ListingId })
				.IsUnique();

			// UserSave
			modelBuilder.Entity<UserSave>()
				.HasOne(us => us.User)
				.WithMany(u => u.Saves)
				.HasForeignKey(us => us.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserSave>()
				.HasOne(us => us.Listing)
				.WithMany()
				.HasForeignKey(us => us.ListingId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserSave>()
				.HasIndex(us => new { us.UserId, us.ListingId })
				.IsUnique();
		}
	}
}