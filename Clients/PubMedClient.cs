using Microsoft.Extensions.Options;
using ResearchPublicationTracker.Clients.Interfaces;
using ResearchPublicationTracker.Clients.Models;
using ResearchPublicatoinsTracker.Options;
using System.Xml.Linq;

namespace ResearchPublicationTracker.Clients
{
	public class PubMedClient(HttpClient httpClient, IOptions<PubMedClientOptions> options) : IPubmedPublicationProvider
	{
		private readonly string apiKey = options.Value.ApiKey;
		private const string SEARCH_FIELDS = "Title/Abstract";
		private const string ARTICLE_TYPES = "Journal+Article";

		public async Task<PubMedESearchResult> GetSearchResult(string query,
			DateTime startDate = default,
			DateTime endDate = default,
			int retstart = 0,
			int retmax = 100,
			int years = 365,
			string? sort = default,
			CancellationToken cancellationToken = default)
		{
			var builder = new PubMedQueryBuilder()
				.SetTerm(query)
				.SetFields(SEARCH_FIELDS)
				.SetArticleTypes(ARTICLE_TYPES)
				.SetRetStart(retstart)
				.SetRetMax(retmax)
				.SetDateRange(startDate.Date, endDate.Date)
				.SetReldateDays(years)
				.SetApiKey(apiKey);

			var searchUrl = builder.BuildESearchUrl();
			var response = await httpClient.GetFromJsonAsync<PubMedESearchResult>(searchUrl, cancellationToken);
			return response ?? new();
		}

		public async Task<PubMedESummaryResult> GetPartialSummaryResult(PubMedESearchResult searchResult,
			CancellationToken cancellationToken = default)
		{
			var result = searchResult.Result;

			var builder = new PubMedQueryBuilder()
				.SetRetStart(result.RetStart)
				.SetRetMax(result.RetMax);

			var summaryUrl = builder.BuildESummaryUrl(result.QueryKey, result.WebEnv);
			var response = await httpClient.GetFromJsonAsync<PubMedESummaryResult>(summaryUrl, cancellationToken);

			return response ?? new();
		}

		public async Task<List<PublicationResult>> GetFullSummaryResult(List<string> ids,
			CancellationToken cancellationToken = default)
		{
			var summaryUrl = new PubMedQueryBuilder().BuildEFetchXMLUrl(ids);
			var responseXml = await httpClient.GetStringAsync(summaryUrl, cancellationToken);
			var doc = XDocument.Parse(responseXml);

			return doc.Descendants("PubmedArticle")
					  .Select(PublicationResultParser.ParsePublication)
					  .Where(pub => pub != null)
					  .ToList()!;
		}

		public async Task<Dictionary<string, int>> GetPublicationCountsOverTime(string term,
			DateTime start,
			DateTime end,
			CancellationToken cancellationToken)
		{
			var results = new Dictionary<string, int>();
			int startYear = start.Year;
			int endYear = end.Year;

			for (int year = startYear; year <= endYear; year++)
			{
				DateTime periodStart = new(year, 1, 1);
				DateTime periodEnd = (year == endYear) ? end.Date : new(year, 12, 31);

				var builder = new PubMedQueryBuilder()
					.SetTerm(term)
					.SetFields(SEARCH_FIELDS)
					.SetArticleTypes(ARTICLE_TYPES)
					.SetDateRange(periodStart, periodEnd)
					.SetRetMax(0)
					.SetApiKey(apiKey);

				string url = builder.BuildESearchUrl();
				var response = await httpClient.GetFromJsonAsync<PubMedESearchResult>(url, cancellationToken);

				int count = response?.Result.Count ?? 0;
				results[year.ToString()] = count;
			}

			return results;
		}

		public async Task<Dictionary<string, int>> GetPublicationTypeDistribution(string term,
			int years,
			CancellationToken cancellationToken)
		{
			var types = new[]
			{
				"Clinical Trial",
				"Review",
				"Meta-Analysis",
				"Case Reports",
				"Randomized Controlled Trial",
				"Journal Article"
			};

			DateTime endDate = DateTime.Now;
			DateTime startDate = new(DateTime.Now.Year - years, 1, 1);

			var results = new Dictionary<string, int>();

			foreach (var type in types)
			{
				var builder = new PubMedQueryBuilder()
					.SetTerm(term)
					.SetFields("Title/Abstract")
					.SetArticleTypes(type.Replace(" ", "+"))
					.SetDateRange(startDate, endDate)
					.SetRetMax(0)
					.SetApiKey(apiKey);

				string url = builder.BuildESearchUrl();
				var response = await httpClient.GetFromJsonAsync<PubMedESearchResult>(url, cancellationToken);
				results[type] = response?.Result.Count ?? 0;
			}

			return results;
		}
	}
}