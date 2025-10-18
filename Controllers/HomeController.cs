using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResearchPublicationTracker.Clients.Interfaces;
using ResearchPublicationTracker.Clients.Models;
using ResearchPublicationTracker.Data;
using ResearchPublicationTracker.Models;
using System.Data;
using System.Linq.Dynamic.Core;
using EFCore.BulkExtensions;

namespace ResearchPublicationTracker.Controllers
{
	public class HomeController(PublicationDbContext dbContext,
		IPubmedPublicationProvider pubmed,
		IScopusPublicationProvider scopus) : Controller
	{
		private const string SCOPUS_PROVIDER = "Scopus";
		private const string PUBMED_PROVIDER = "PubMed";

		private readonly DateTime StartDate = new(DateTime.Now.Year - 4, 1, 1);
		private readonly DateTime EndDate = DateTime.Now;

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> SearchPubmed(DataTableRequest request, CancellationToken cancellationToken)
		{
			if (!IsValidSearch(request)) return EmptyDataTableResponse(request);

			var searchTerm = request.SearchTerm;
			var searchResult = await GetPubmedDataAsync(request, cancellationToken);
			var publications = BuildBaseQuery(PUBMED_PROVIDER, searchTerm);



			var totalRecords = searchResult.Result.Count;
			var filteredRecords = searchResult.Result.Count;

			//filtering
			if (!string.IsNullOrEmpty(request.Search.Value))
			{
				var searchValue = request.Search.Value.ToLower().Trim();
				var tCount = await publications.CountAsync(cancellationToken);
				totalRecords = tCount;

				publications = publications.Where(p => EF.Functions.Like(p.Title.ToLower(), $"%{searchValue}%") ||
													   EF.Functions.Like(p.Abstract.ToLower(), $"%{searchValue}%") ||
													   p.Authors.Any(x => EF.Functions.Like(x.Name.ToLower(), $"%{searchValue}%")));

				var fCount = await publications.CountAsync(cancellationToken);
				filteredRecords = fCount;
			}

			publications = publications.OrderByDescending(p => p.PublicationDate);

			//sorting
			//if (dtr.Order != null && dtr.Order.Count > 0)
			//{
			//	var orderBy = dtr.Order.Select(o => $"{dtr.Columns[o.Column].Data} {o.Dir}");
			//	publications = publications.OrderBy(string.Join(", ", orderBy));
			//}

			//paging
			var pageSize = request.Length;
			var skip = request.Start;

			var publicationsData = await publications
				.Select(p => new
				{
					p.Id,
					p.Provider,
					p.ProviderId,
					p.Title,
					PublicationDate = p.PublicationDate.ToShortDateString(),
					p.Authors,
					p.Abstract,
					p.RecordUrl
				}).Skip(skip).Take(pageSize).ToListAsync(cancellationToken);

			var returnObject = new
			{
				draw = request.Draw,
				recordsTotal = totalRecords,
				recordsFiltered = filteredRecords,
				data = publicationsData
			};

			return Json(returnObject);
		}

		public async Task<IActionResult> SearchScopus(DataTableRequest request, CancellationToken cancellationToken)
		{
			if (!IsValidSearch(request)) return EmptyDataTableResponse(request);

			var searchTerm = request.SearchTerm;

			var searchResult = await GetScopusDataAsync(request, cancellationToken);
			var publications = BuildBaseQuery(SCOPUS_PROVIDER, searchTerm);

			var totalRecords = searchResult.Results.TotalResults;
			var filteredRecords = searchResult.Results.TotalResults;

			//filtering	
			if (!string.IsNullOrEmpty(request.Search.Value))
			{
				var searchValue = request.Search.Value.ToLower().Trim();
				var tCount = await publications.CountAsync(cancellationToken);
				totalRecords = tCount.ToString();

				publications = publications.Where(p => EF.Functions.Like(p.Title.ToLower(), $"%{searchValue}%") ||
													   EF.Functions.Like(p.Abstract.ToLower(), $"%{searchValue}%") ||
													   p.Authors.Any(x => x.Name.ToLower() == searchValue));

				var fCount = await publications.CountAsync(cancellationToken);
				filteredRecords = fCount.ToString();
			}

			publications = publications.OrderByDescending(p => p.PublicationDate);

			//sorting
			//if (dtr.Order != null && dtr.Order.Count > 0)
			//{
			//	var orderBy = dtr.Order.Select(o => $"{dtr.Columns[o.Column].Data} {o.Dir}");
			//	publications = publications.OrderBy(string.Join(", ", orderBy));
			//}

			//paging
			var pageSize = request.Length;
			var skip = request.Start;

			var publicationsData = await publications
				.Select(p => new PublicatoinSummary
				{
					Id = p.Id,
					Provider = p.Provider,
					ProviderId = p.ProviderId,
					Title = p.Title,
					PublicationDate = p.PublicationDate.ToShortDateString(),
					Authors = p.Authors,
					Abstract = p.Abstract,
					RecordUrl = p.RecordUrl
				}).Skip(skip).Take(pageSize).ToListAsync(cancellationToken);

			var listToUpdate = new List<Publication>();

			foreach (var pub in publicationsData)
			{
				if (pub.Abstract.Equals("No abstract available"))
				{
					var fullPub = await scopus.GetPublications([pub.ProviderId], cancellationToken);
					var abstractText = fullPub.FirstOrDefault()!.Abstract;
					publicationsData.First(p => p.Id == pub.Id).Abstract = abstractText;

					var pubToUpdate = await dbContext.Publications.FirstAsync(p => p.Id == pub.Id, cancellationToken);
					pubToUpdate.Abstract = abstractText;
					listToUpdate.Add(pubToUpdate);
				}
			}

			await dbContext.BulkUpdateAsync(listToUpdate, cancellationToken: cancellationToken);


			var returnObject = new
			{
				draw = request.Draw,
				recordsTotal = totalRecords,
				recordsFiltered = filteredRecords,
				data = publicationsData
			};

			return Json(returnObject);
		}

		private bool IsValidSearch(DataTableRequest request)
		{
			return !string.IsNullOrWhiteSpace(request.SearchTerm) && request.Start <= 9999;
		}

		private JsonResult EmptyDataTableResponse(DataTableRequest request)
		{
			return new(new
			{
				draw = request.Draw,
				recordsTotal = 0,
				recordsFiltered = 0,
				data = new List<object>()
			});
		}

		private IQueryable<Publication> BuildBaseQuery(string provider, string searchTerm)
		{
			return dbContext.Publications
			   .Where(p => p.Provider == provider &&
			   EF.Functions.Like(p.SearchTerms!.ToLower(), $"%{searchTerm.ToLower()}%"));
		}

		[HttpGet]
		public async Task<IActionResult> GetScopusAbstract(string scopusId, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(scopusId)) return BadRequest();
			var publication = await scopus.GetPublications([scopusId], cancellationToken);

			var pub = dbContext.Publications
				.FirstOrDefault(p => p.ProviderId == scopusId && p.Provider == "Scopus");
			pub!.Abstract = publication[0].Abstract;
			dbContext.Publications.Update(pub);
			await dbContext.SaveChangesAsync(cancellationToken);
			return Json(publication[0]);
		}

		private async Task<PubMedESearchResult> GetPubmedDataAsync(DataTableRequest form,
			CancellationToken cancellationToken)
		{
			var searchResult = await pubmed.GetSearchResult(
				form.SearchTerm,
				StartDate,
				EndDate,
				form.Start,
				retmax: 100,
				cancellationToken: cancellationToken);

			var searchResultIds = searchResult.Result.IdList;
			var provider = PUBMED_PROVIDER;
			var newTerm = form.SearchTerm.Trim();

			// Fetch all existing publications that match those IDs
			var existingPublications = await dbContext.Publications
				.Include(p => p.Authors)
				.Where(p => p.Provider == provider && searchResultIds.Contains(p.ProviderId))
				.ToListAsync(cancellationToken);

			var existingIds = existingPublications.Select(p => p.ProviderId).ToHashSet();

			// Update search terms in existing pubs
			foreach (var pub in existingPublications)
			{
				var existingTerms = (pub.SearchTerms ?? string.Empty)
					.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
					.ToList();

				if (!existingTerms.Contains(newTerm, StringComparer.OrdinalIgnoreCase))
				{
					existingTerms.Add(newTerm);
					pub.SearchTerms = string.Join(", ", existingTerms);
				}
			}

			// Bulk update all modified ones
			if (existingPublications.Count > 0)
				await dbContext.BulkUpdateAsync(existingPublications, cancellationToken: cancellationToken);

			// Find new ones
			var newIds = searchResultIds.Except(existingIds).ToList();
			if (newIds.Count > 0)
			{
				var newFullData = await pubmed.GetFullSummaryResult(newIds, cancellationToken);

				var newPublications = newFullData.Select(pubResult => new Publication
				{
					Provider = provider,
					ProviderId = pubResult.ProviderId,
					Title = pubResult.Title,
					Abstract = pubResult.Abstract,
					PublicationDate = pubResult.PublicationDate,
					RecordUrl = pubResult.RecordUrl,
					Authors = [.. pubResult.Authors.Select((x, i) => new PublicationAuthor
					{
						AuthorOrder = i + 1,
						Name = x
					})],
					SearchTerms = newTerm
				}).ToList();


				// Bulk insert new records
				if (newPublications.Count > 0)
				{
					await dbContext.AddRangeAsync(newPublications, cancellationToken: cancellationToken);
				}
			}

			await dbContext.SaveChangesAsync(cancellationToken);
			return searchResult;
		}

		private async Task<ScopusSearchResult> GetScopusDataAsync(DataTableRequest form,
			CancellationToken cancellationToken)
		{
			var searchResult = await scopus.GetSearchResult(
				form.SearchTerm,
				form.Start,
				100,
				fromYear: StartDate.Year,
				toYear: EndDate.Year + 1,
				cancellationToken: cancellationToken);

			var provider = SCOPUS_PROVIDER;
			var searchResultIds = searchResult.Results.Entries
				.Select(x => x.Identifier)
				.Distinct()
				.ToList();

			if (searchResultIds.Count == 0)
				return searchResult;

			// --- Load existing ---
			var existingPublications = await dbContext.Publications
				.Include(p => p.Authors)
				.Where(p => p.Provider == provider && searchResultIds.Contains(p.ProviderId))
				.ToListAsync(cancellationToken);

			var existingIds = existingPublications
				.Select(p => p.ProviderId)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			var newTerm = form.SearchTerm.Trim();

			// --- Update existing search terms ---
			foreach (var pub in existingPublications)
			{
				var terms = (pub.SearchTerms ?? string.Empty)
					.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
					.ToList();

				if (!terms.Contains(newTerm, StringComparer.OrdinalIgnoreCase))
				{
					terms.Add(newTerm);
					pub.SearchTerms = string.Join(", ", terms);
				}
			}

			if (existingPublications.Count > 0)
				await dbContext.BulkUpdateAsync(existingPublications, cancellationToken: cancellationToken);

			// --- Build new publications ---
			var newPublications = searchResult.Results.Entries
				.Where(e => !existingIds.Contains(e.Identifier!))
				.Select(ScopusPublicationParser.Parse!)
				.Where(p => p != null)
				.Select(result => new Publication
				{
					Provider = provider,
					ProviderId = result.ProviderId,
					Title = result.Title,
					Abstract = result.Abstract,
					PublicationDate = result.PublicationDate,
					RecordUrl = $"https://www.scopus.com/pages/publications/{result.ProviderId}",
					Authors = [.. result.Authors.Select((a, i) => new PublicationAuthor
					{
						AuthorOrder = i + 1,
						Name = a
					})],
					SearchTerms = newTerm
				})
				.ToList();

			if (newPublications.Count > 0)
			{
				// Double-check duplicates before bulk insert
				var duplicateIds = await dbContext.Publications
					.Where(p => p.Provider == provider && newPublications.Select(np => np.ProviderId).Contains(p.ProviderId))
					.Select(p => p.ProviderId)
					.ToListAsync(cancellationToken);

				newPublications = [.. newPublications.Where(np => !duplicateIds.Contains(np.ProviderId))];

				if (newPublications.Count > 0)
				{
					await dbContext.AddRangeAsync(newPublications, cancellationToken: cancellationToken);
				}
			}

			await dbContext.SaveChangesAsync(cancellationToken);
			return searchResult;
		}

		public async Task<IActionResult> GetV1(string search, CancellationToken cancellationToken)
		{
			var data = await pubmed.GetPublicationCountsOverTime(search, StartDate, EndDate, cancellationToken);
			return Json(data);
		}

		public async Task<IActionResult> GetV2(string search, CancellationToken cancellationToken)
		{
			var data = await pubmed.GetPublicationTypeDistribution(search, 4, cancellationToken);
			return Json(data);
		}

		public async Task<IActionResult> GetScopurPublicatoinsCountOverTime(string search,
			CancellationToken cancellationToken)
		{
			var data = await scopus.GetPublicationCountsOverTime(search, StartDate.Year, EndDate.Year, cancellationToken);
			return Json(data);
		}

		public async Task<IActionResult> GetScopusPublicationTypeDistribution(string search, CancellationToken cancellationToken)
		{
			var data = await scopus.GetPublicationTypeDistribution(search, 4, cancellationToken);
			return Json(data);
		}
	}

	internal class PublicatoinSummary
	{
		public int Id { get; set; }
		public string Provider { get; set; } = null!;
		public string ProviderId { get; set; } = null!;
		public string Title { get; set; } = null!;
		public string PublicationDate { get; set; } = null!;
		public List<PublicationAuthor> Authors { get; set; } = [];
		public string Abstract { get; set; } = null!;
		public string? RecordUrl { get; set; }
	}
}
