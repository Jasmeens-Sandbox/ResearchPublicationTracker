using ResearchPublicationTracker.Clients.Models;

namespace ResearchPublicationTracker.Clients.Interfaces
{
	public interface IPubmedPublicationProvider
	{

		Task<PubMedESearchResult> GetSearchResult(string query,
			DateTime startDate = default,
			DateTime endDate = default,
			int retstart = 0,
			int retmax = 100,
			int years = 365,
			string? sort = null,
			CancellationToken cancellationToken = default);

		Task<List<PublicationResult>> GetFullSummaryResult(List<string> ids,
			CancellationToken cancellationToken);

		Task<PubMedESummaryResult> GetPartialSummaryResult(PubMedESearchResult searchResult,
			CancellationToken cancellationToken);

		Task<Dictionary<string, int>> GetPublicationCountsOverTime(string term,
			DateTime startDate,
			DateTime endDate,
			CancellationToken cancellationToken);

		Task<Dictionary<string, int>> GetPublicationTypeDistribution(string term,
			int years,
			CancellationToken cancellationToken);
	}
}