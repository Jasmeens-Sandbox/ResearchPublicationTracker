using ResearchPublicationTracker.Clients.Models;

namespace ResearchPublicationTracker.Clients.Interfaces
{
	public interface IScopusPublicationProvider
	{
		Task<ScopusSearchResult> GetSearchResult(string query,
			int start = 0,
			int count = 25,
			string? sort = null,
			int? fromYear = null,
			int? toYear = null,
			CancellationToken cancellationToken = default);

		Task<List<PublicationResult>> GetPublications(List<string> ids,
			CancellationToken cancellationToken);

		Task<Dictionary<string, int>> GetPublicationCountsOverTime(string query,
			int fromYear,
			int toYear,
			CancellationToken cancellationToken);
		Task<Dictionary<string, int>> GetPublicationTypeDistribution(string search, 
			int years, 
			CancellationToken cancellationToken);
	}

}
