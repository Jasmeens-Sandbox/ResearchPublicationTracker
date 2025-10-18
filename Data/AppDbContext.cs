using Microsoft.EntityFrameworkCore;
using ResearchPublicationTracker.Models;

namespace ResearchPublicationTracker.Data
{
	public class PublicationDbContext(DbContextOptions options) : DbContext(options)
	{
		public DbSet<Publication> Publications { get; set; } = null!;
		public DbSet<PublicationAuthor> PublicationAuthors { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Unique publication constraint
			modelBuilder.Entity<Publication>()
				.HasIndex(p => new { p.Provider, p.ProviderId })
				.IsUnique();

			// One-to-many: Publication -> PublicationAuthors
			modelBuilder.Entity<PublicationAuthor>()
				.HasOne(pa => pa.Publication)
				.WithMany(p => p.Authors)
				.HasForeignKey(pa => pa.PublicationId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
