using System.Xml.Linq;

namespace ResearchPublicationTracker.Clients.Models
{
	public class PublicationResult
	{
		public string Title { get; set; } = null!;
		public string Provider { get; set; } = null!;
		public string ProviderId { get; set; } = null!;
		public string RecordUrl { get; set; } = null!;
		public string Url{ get; set; }
		public string Abstract { get; set; } = null!;
		public List<string> Authors { get; set; } = [];
		public DateTime PublicationDate { get; set; }
	}

	public class PublicationResultParser 
	{
		public static PublicationResult? ParsePublication(XElement article)
		{
			var pubDateStr = ParsePubDate(article);
			var providerId = article.Descendants("PMID").FirstOrDefault()?.Value;

			if (string.IsNullOrEmpty(providerId) || !DateTime.TryParse(pubDateStr, out var pubDate))
				return null;

			var recordUrl = $"https://pubmed.ncbi.nlm.nih.gov/{providerId}/";
			var title = article.Descendants("ArticleTitle").FirstOrDefault()?.Value ?? "No title available";
			var abstractText = ParseAbstract(article);
			var authors = ParseAuthors(article);

			return new()
			{
				Title = title,
				Authors = authors,
				Provider = "PubMed",
				RecordUrl = recordUrl,
				Abstract = abstractText,
				ProviderId = providerId,
				PublicationDate = pubDate
			};

			List<string> ParseAuthors(XElement author)
			{
				return [.. article.Descendants("Author")
							.Select(ParseName)
							.Where(name => !string.IsNullOrWhiteSpace(name))];

				string ParseName(XElement author)
				{
					return $"{author.Element("LastName")?.Value} {author.Element("Initials")?.Value}".Trim();
				}
			}

			string ParsePubDate(XElement article)
			{
				var pubDate = article.Descendants("PubMedPubDate")
							 .FirstOrDefault(x => x?.Attribute("PubStatus")?.Value == "pubmed");

				var day = pubDate?.Element("Day")?.Value;
				var month = pubDate?.Element("Month")?.Value;
				var year = pubDate?.Element("Year")?.Value;

				return $"{day}-{month}-{year}";
			}

			string ParseAbstract(XElement article)
			{
				var abstractElem = article.Descendants("Abstract")
										  .Select(x => x.Value)
										  .Where(x => !string.IsNullOrWhiteSpace(x));

				var abstractText = string.Join(" ", abstractElem);

				if (!string.IsNullOrWhiteSpace(abstractText))
					return abstractText;

				return "No abstract available";
			}
		}
	}
}