namespace ResearchPublicationTracker.Models
{
	public class Publication
	{
		public int Id { get; set; }
		public string Title { get; set; } = null!;
		public string Abstract { get; set; } = null!;
		public string? RecordUrl { get; set; }
		public string Provider { get; set; } = null!;
		public string ProviderId { get; set; } = null!;
		public string? SearchTerms { get; set; } = null!;
		public DateTime PublicationDate { get; set; }
		public List<PublicationAuthor> Authors { get; set; } = [];
	}
}
