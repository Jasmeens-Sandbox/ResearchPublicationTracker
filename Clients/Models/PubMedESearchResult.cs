using System.Text.Json.Serialization;

namespace ResearchPublicationTracker.Clients.Models
{
	public class PubMedESearchResult
	{
		[JsonPropertyName("esearchresult")]
		public ESearchResult Result { get; set; } = new();
	}

	public class ESearchResult
	{
		private int count;

		[JsonPropertyName("count")]
		public int Count 
		{ 
			get => count;
			set => count = value; //Math.Clamp(value, 0, 9999);
		}

		[JsonPropertyName("idlist")]
		public List<string> IdList { get; set; } = [];

		[JsonPropertyName("retmax")]
		public int RetMax { get; set; }

		[JsonPropertyName("retstart")]
		public int RetStart { get; set; }

		[JsonPropertyName("webenv")]
		public string WebEnv { get; set; } = null!;

		[JsonPropertyName("querykey")]
		public string QueryKey { get; set; } = null!;
	}
}
