using System.Text.Json;
using System.Text.Json.Serialization;

namespace ResearchPublicationTracker.Clients.Models
{
	public class PubMedESummaryResult
	{
		[JsonPropertyName("result")]
		public ESummaryResult Result { get; set; } = new();
	}

	public class ESummaryResult
	{
		[JsonPropertyName("uids")]
		public List<string> Uids { get; set; } = [];

		[JsonExtensionData]
		public Dictionary<string, JsonElement> Articles { get; set; } = [];
	}

	public class Article
	{
		[JsonPropertyName("uid")]
		public string Uid { get; set; } = null!;

		[JsonPropertyName("pubdate")]
		public string PubDate { get; set; } = null!;

		[JsonPropertyName("epubdate")]
		public string EPubDate { get; set; } = null!;

		[JsonPropertyName("source")]
		public string Source { get; set; } = null!;

		[JsonPropertyName("authors")]
		public List<Author> Authors { get; set; } = [];

		[JsonPropertyName("lastauthor")]
		public string LastAuthor { get; set; } = null!;

		[JsonPropertyName("title")]
		public string Title { get; set; } = null!;

		[JsonPropertyName("sorttitle")]
		public string SortTitle { get; set; } = null!;

		[JsonPropertyName("volume")]
		public string Volume { get; set; } = null!;

		[JsonPropertyName("issue")]
		public string Issue { get; set; } = null!;

		[JsonPropertyName("pages")]
		public string Pages { get; set; } = null!;

		[JsonPropertyName("lang")]
		public List<string> Lang { get; set; } = [];

		[JsonPropertyName("nlmuniqueid")]
		public string NlmUniqueId { get; set; } = null!;

		[JsonPropertyName("issn")]
		public string ISSN { get; set; } = null!;

		[JsonPropertyName("essn")]
		public string EISSN { get; set; } = null!;

		[JsonPropertyName("pubtype")]
		public List<string> PubType { get; set; } = [];

		[JsonPropertyName("recordstatus")]
		public string RecordStatus { get; set; } = null!;

		[JsonPropertyName("pubstatus")]
		public string PubStatus { get; set; } = null!;

		[JsonPropertyName("articleids")]
		public List<ArticleId> ArticleIds { get; set; } = [];

		[JsonPropertyName("history")]
		public List<History> History { get; set; } = [];

		[JsonPropertyName("references")]
		public List<object> References { get; set; } = [];

		[JsonPropertyName("attributes")]
		public List<string> Attributes { get; set; } = [];

		[JsonPropertyName("pmcrefcount")]
		public object PmcRefCount { get; set; } = null!;

		[JsonPropertyName("fulljournalname")]
		public string FullJournalName { get; set; } = null!;

		[JsonPropertyName("elocationid")]
		public string ELocationId { get; set; } = null!;

		[JsonPropertyName("doctype")]
		public string DocType { get; set; } = null!;

		[JsonPropertyName("sortpubdate")]
		public string SortPubDate { get; set; } = null!;

		[JsonPropertyName("sortfirstauthor")]
		public string SortFirstAuthor { get; set; } = null!;

		[JsonPropertyName("vernaculartitle")]
		public string VernacularTitle { get; set; } = null!;
	}

	public class Author
	{
		[JsonPropertyName("name")]
		public string Name { get; set; } = null!;

		[JsonPropertyName("authtype")]
		public string AuthType { get; set; } = null!;

		[JsonPropertyName("clusterid")]
		public string ClusterId { get; set; } = null!;
	}

	public class ArticleId
	{
		[JsonPropertyName("idtype")]
		public string IdType { get; set; } = null!;

		[JsonPropertyName("idtypen")]
		public int IdTypeN { get; set; } = 0;

		[JsonPropertyName("value")]
		public string Value { get; set; } = null!;
	}

	public class History
	{
		[JsonPropertyName("pubstatus")]
		public string PubStatus { get; set; } = null!;

		[JsonPropertyName("date")]
		public string Date { get; set; } = null!;
	}
}
