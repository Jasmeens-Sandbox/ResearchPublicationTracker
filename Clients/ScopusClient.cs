using Microsoft.Extensions.Options;
using ResearchPublicationTracker.Clients.Interfaces;
using ResearchPublicationTracker.Clients.Models;
using ResearchPublicationTracker.Options;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ResearchPublicationTracker.Clients
{
	public class ScopusClient : IScopusPublicationProvider
	{
		private readonly HttpClient httpClient;

		public ScopusClient(HttpClient httpClient, IOptions<ScopusClientOptions> options)
		{
			httpClient.DefaultRequestHeaders.Add("X-ELS-APIKey", options.Value.ApiKey);
			httpClient.DefaultRequestHeaders.Add("X-ELS-Insttoken", options.Value.InstToken);
			httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			this.httpClient = httpClient;
		}

		public async Task<ScopusSearchResult> GetSearchResult(
			string query,
			int start = 0,
			int count = 25,
			string? sort = null,
			int? fromYear = null,
			int? toYear = null,
			CancellationToken cancellationToken = default)
		{
			var builder = new ScopusQueryBuilder()
				.SetTitleAndAbstractQuery(query)
				.SetYearRange(fromYear, toYear)
				.SetPublicationType("journal")
				.SetStart(start)
				.SetCount(count)
				.SetSort(sort);

			var url = builder.BuildUrl();
			var result = await httpClient.GetFromJsonAsync<ScopusSearchResult>(url, cancellationToken);
			return result ?? new();
		}

		public async Task<List<PublicationResult>> GetPublications(List<string> ids,
			CancellationToken cancellationToken)
		{
			var results = new List<PublicationResult>();

			foreach (var id in ids)
			{
				await Task.Delay(100, cancellationToken); // Rate limit

				string url = $"https://api.elsevier.com/content/abstract/scopus_id/{id}" +
							 $"?httpAccept=application/json";

				var response = await httpClient.GetFromJsonAsync<ScopusAbstractResponse>(url, cancellationToken);

				if (response?.RetrievalResponse != null)
				{
					var core = response.RetrievalResponse.CoreData;
					var abstractText = response.RetrievalResponse.Item?.BibRecord?.Head?.Abstracts;

					results.Add(new PublicationResult
					{
						Title = core.Title,
						Provider = "Scopus",
						ProviderId = core.ScopusId,
						RecordUrl = core.RecordUrl,
						PublicationDate = DateTime.TryParse(core.CoverDate, out var dt) ? dt : default,
						Abstract = string.IsNullOrWhiteSpace(abstractText)
								   ? $"No abstract available (type: {core.SubtypeDescription})"
								   : abstractText,
						Authors = [],
						Url = core.DOI != null ? $"https://doi.org/{core.DOI}" : string.Empty
					});
				}
			}

			return results;
		}

		public async Task<Dictionary<string, int>> GetPublicationCountsOverTime(
			string query,
			int fromYear,
			int toYear,
			CancellationToken cancellationToken)
		{
			var result = new Dictionary<string, int>();

			for (int year = fromYear; year <= toYear; year++)
			{
				var builder = new ScopusQueryBuilder()
					.SetTitleAndAbstractQuery(query)
					.SetPublicationType("journal")
					.SetYear(year)
					.SetCount(0);

				string url = builder.BuildUrl();
				var response = await httpClient.GetFromJsonAsync<ScopusSearchResult>(url, cancellationToken);

				if (response?.Results?.TotalResults != null &&
					int.TryParse(response.Results.TotalResults, out int count))
				{
					result[year.ToString()] = count;
				}
				else
				{
					result[year.ToString()] = 0;
				}
			}

			return result;
		}

		public async Task<Dictionary<string, int>> GetPublicationTypeDistribution(string search,
			int years,
			CancellationToken cancellationToken)
		{
			var types = new[]
			{
				"Journal",
				"Conference",
				"Book",
				"Book Series",
			};

			DateTime endDate = DateTime.Now;
			DateTime startDate = new(DateTime.Now.Year - years, 1, 1);

			var results = new Dictionary<string, int>();

			foreach (var type in types)
			{
				var builder = new ScopusQueryBuilder()
					.SetTitleAndAbstractQuery(search)
					.SetPublicationType(type.ToLower())
					.SetYearRange(startDate.Year, endDate.Year)
					.SetCount(0);

				string url = builder.BuildUrl();
				var response = await httpClient.GetFromJsonAsync<ScopusSearchResult>(url, cancellationToken);
				_ = int.TryParse(response?.Results?.TotalResults ?? "0", out int count);
				results[type] = count;
			}

			return results;
		}
	}
}
