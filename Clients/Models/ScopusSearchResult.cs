using System.Text.Json.Serialization;

namespace ResearchPublicationTracker.Clients.Models
{
	public class ScopusSearchResult
	{
		[JsonPropertyName("search-results")]
		public ScopusSearchResults Results { get; set; } = new();
	}

	public class ScopusSearchResults
	{
		[JsonPropertyName("entry")]
		public List<ScopusEntry> Entries { get; set; } = [];

		[JsonPropertyName("opensearch:totalResults")]
		public string? TotalResults { get; set; }
	}

	public class ScopusEntry
	{
		[JsonPropertyName("dc:title")]
		public string? Title { get; set; }

		[JsonPropertyName("dc:identifier")]
		public string? Identifier { get; set; }

		[JsonPropertyName("prism:publicationName")]
		public string? Source { get; set; }

		[JsonPropertyName("prism:coverDate")]
		public string? PublicationDate { get; set; }

		[JsonPropertyName("dc:creator")]
		public string? Creator { get; set; }

		[JsonPropertyName("dc:description")]
		public string? Abstract { get; set; }

		[JsonPropertyName("prism:url")]
		public string? RecordUrl { get; set; }
	}

	public static class ScopusPublicationParser
	{
		public static PublicationResult Parse(ScopusEntry entry)
		{
			var id = entry.Identifier?.Replace("SCOPUS_ID:", "") ?? "";
			var pubDate = DateTime.TryParse(entry.PublicationDate, out var dt) ? dt : DateTime.MinValue;


			return new PublicationResult
			{
				Title = entry.Title ?? "No title",
				Abstract = entry.Abstract ?? "No abstract available",
				Provider = "Scopus",
				ProviderId = id,
				//RecordUrl = $"https://www.scopus.com/record/display.uri?eid={id}",
				RecordUrl = $"https://www.scopus.com/pages/publications/{id}",
				Url = entry.RecordUrl,
				Authors = new List<string> { entry.Creator ?? "Unknown" },
				PublicationDate = pubDate,			
			};
		}
	}
}
