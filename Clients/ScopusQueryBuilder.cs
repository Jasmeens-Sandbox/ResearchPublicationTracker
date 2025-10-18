using System.Text;

namespace ResearchPublicationTracker.Clients
{
	public class ScopusQueryBuilder
	{
		private const string BASE_URL = "https://api.elsevier.com/content/search/scopus";
		private string _query = string.Empty;
		private int _start = 0;
		private int _count = 25;
		private string? _sort = null;
		private string? _view = "STANDARD";
		private string _apiKey = string.Empty;

		public ScopusQueryBuilder SetQuery(string query)
		{
			_query = query.Trim();
			return this;
		}

		public ScopusQueryBuilder SetTitleAndAbstractQuery(string term)
		{
			_query = $"TITLE-ABS-KEY({term})";
			return this;
		}

		public ScopusQueryBuilder SetYearRange(int? fromYear = null, int? toYear = null)
		{
			var filters = new List<string>();
			if (fromYear.HasValue)
				filters.Add($"PUBYEAR AFT {fromYear.Value}");
			if (toYear.HasValue)
				filters.Add($"PUBYEAR BEF {toYear.Value}");

			if (filters.Count > 0)
				_query = string.IsNullOrEmpty(_query)
					? string.Join(" AND ", filters)
					: $"{_query} AND {string.Join(" AND ", filters)}";

			return this;
		}

		public ScopusQueryBuilder SetYear(int year)
		{
			var filter = $"PUBYEAR = {year}";

			_query = string.IsNullOrEmpty(_query)
				? filter
				: $"{_query} AND {filter}";

			return this;
		}

		public ScopusQueryBuilder SetPublicationType(string type)
		{
			if (string.IsNullOrWhiteSpace(type))
				return this;

			type = type.Trim().ToLowerInvariant();

			// Map human-friendly names to Scopus SRCTYPE codes
			string srcType = type switch
			{
				"journal" or "journals" => "SRCTYPE(j)",
				"conference" or "conference proceeding" or "proceedings" => "SRCTYPE(p)",
				"book" or "books" => "SRCTYPE(b)",
				"book series" or "series" => "SRCTYPE(k)",
				_ => string.Empty
			};

			_query = string.IsNullOrEmpty(_query)
				? srcType
				: $"{_query} AND {srcType}";

			return this;
		}

		public ScopusQueryBuilder SetStart(int start)
		{
			_start = start;
			return this;
		}

		public ScopusQueryBuilder SetCount(int count)
		{
			_count = count;
			return this;
		}

		public ScopusQueryBuilder SetSort(string? sort)
		{
			_sort = sort;
			return this;
		}

		public ScopusQueryBuilder SetApiKey(string key)
		{
			_apiKey = key;
			return this;
		}

		public ScopusQueryBuilder SetView(string? view)
		{
			_view = view;
			return this;
		}

		public string BuildUrl()
		{
			if (string.IsNullOrWhiteSpace(_query))
				throw new InvalidOperationException("Scopus query cannot be empty.");

			var sb = new StringBuilder(BASE_URL);
			sb.Append("?query=").Append(Uri.EscapeDataString(_query));
			sb.Append("&start=").Append(_start);
			sb.Append("&count=").Append(_count);
			sb.Append("&view=").Append(_view ?? "STANDARD");

			if (!string.IsNullOrEmpty(_sort))
				sb.Append("&sort=").Append(Uri.EscapeDataString(_sort));

			return sb.ToString();
		}		
	}
}
