using System.Text.Json.Serialization;

namespace ResearchPublicationTracker.Clients.Models
{
	public class ScopusAbstractResponse
	{
		[JsonPropertyName("abstracts-retrieval-response")]
		public AbstractsRetrievalResponse RetrievalResponse { get; set; } = new();
	}

	public class AbstractsRetrievalResponse
	{
		[JsonPropertyName("coredata")]
		public CoreData CoreData { get; set; } = new();

		[JsonPropertyName("item")]
		public AbstractItem? Item { get; set; }
	}

	public class CoreData
	{
		[JsonPropertyName("dc:title")]
		public string Title { get; set; } = null!;

		[JsonPropertyName("prism:doi")]
		public string? DOI { get; set; }

		[JsonPropertyName("prism:url")]
		public string RecordUrl { get; set; } = null!;

		[JsonPropertyName("dc:identifier")]
		public string ScopusId { get; set; } = null!;

		[JsonPropertyName("prism:coverDate")]
		public string CoverDate { get; set; } = null!;

		[JsonPropertyName("subtypeDescription")]
		public string SubtypeDescription { get; set; } = null!;
	}

	public class AbstractItem
	{
		[JsonPropertyName("bibrecord")]
		public BibRecord BibRecord { get; set; } = new();
	}

	public class BibRecord
	{
		[JsonPropertyName("head")]
		public BibHead Head { get; set; } = new();
	}

	public class BibHead
	{
		[JsonPropertyName("abstracts")]
		public string? Abstracts { get; set; }

		[JsonPropertyName("citation-title")]
		public string Title { get; set; } = null!;
	}
}
