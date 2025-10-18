public class PubMedQueryBuilder
{
	private string _term = string.Empty;
	private string[] _fields = [];
	private string _articleType = string.Empty;
	private int _retStart = 0;
	private int _retMax = 100;
	private int? _reldateDays = null;
	private string? _sort = null;
	private string _apiKey = "";
	private DateTime? _mindate = null;
	private DateTime? _maxdate = null;

	private const string BASE_URL = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/";
	private const string ESEARCH_ENDPOINT = $"{BASE_URL}esearch.fcgi";
	private const string ESUMMARY_ENDPOINT = $"{BASE_URL}esummary.fcgi";
	private const string EFETCH_ENDPOINT = $"{BASE_URL}efetch.fcgi";

	public PubMedQueryBuilder SetTerm(string term)
	{
		_term = $"\"{term.Trim().Replace(" ", "+")}\"";
		return this;
	}

	public PubMedQueryBuilder SetFields(params string[] fields)
	{
		if (fields is { Length: > 0 })
			_fields = fields;

		return this;
	}

	public PubMedQueryBuilder SetArticleTypes(params string[] types)
	{
		if (types is { Length: > 0 })
			_articleType = string.Join(" OR ", types.Select(t => $"{t}[pt]"));
		return this;
	}

	public PubMedQueryBuilder SetRetStart(int retStart)
	{
		_retStart = retStart;
		return this;
	}

	public PubMedQueryBuilder SetRetMax(int retMax)
	{
		_retMax = retMax;
		return this;
	}

	public PubMedQueryBuilder SetReldateDays(int days)
	{
		_reldateDays = days;
		return this;
	}

	public PubMedQueryBuilder SetSort(string sort)
	{
		_sort = sort;
		return this;
	}

	public PubMedQueryBuilder SetApiKey(string apiKey)
	{
		_apiKey = apiKey;
		return this;
	}

	public PubMedQueryBuilder SetDateRange(DateTime start, DateTime end)
	{
		_mindate = start;
		_maxdate = end;
		return this;
	}

	public string BuildESearchUrl()
	{
		if (string.IsNullOrWhiteSpace(_term))
			throw new ArgumentException("Search term cannot be empty.", nameof(_term));

		var fieldParts = _fields.Select(f => $"{_term}[{f}]");
		var fieldsQuery = $"({string.Join(" OR ", fieldParts)})";

		var query = $"{fieldsQuery} AND {_articleType}";

		var url = $"{ESEARCH_ENDPOINT}" +
				  $"?db=pubmed" +
				  $"&term={query}" +
				  $"&retmode=json" +
				  $"&usehistory=y" +
				  $"&retstart={_retStart}" +
				  $"&retmax={_retMax}";

		if (!string.IsNullOrEmpty(_sort))
		{
			url += $"&sort={_sort}";
		}

		if (!string.IsNullOrEmpty(_apiKey))
		{
			url += $"&api_key={_apiKey}";
		}

		if (_mindate != default && _maxdate != default)
		{
			url += $"&datetype=crdt" +
				   $"&mindate={_mindate:yyyy/MM/dd}" +
				   $"&maxdate={_maxdate:yyyy/MM/dd}";
		}
		else if (_reldateDays.HasValue)
		{
			url += $"&reldate={_reldateDays.Value}";
		}

		return url;
	}

	public string BuildESummaryUrl(string queryKey, string webEnv)
	{
		var url = $"{ESUMMARY_ENDPOINT}" +
				  $"?db=pubmed" +
				  $"&query_key={queryKey}" +
				  $"&WebEnv={webEnv}" +
				  $"&retmode=json" +
				  $"&retstart={_retStart}" +
				  $"&retmax={_retMax}";

		if (!string.IsNullOrEmpty(_sort))
			url += $"&sort={_sort}";

		if (!string.IsNullOrEmpty(_apiKey))
			url += $"&api_key={_apiKey}";

		return url;
	}

	public string BuildEFetchJsonUrl(IEnumerable<string> ids)
	{
		if (ids == null || !ids.Any())
			throw new ArgumentException("IDs cannot be null or empty.", nameof(ids));

		var idList = string.Join(",", ids);
		var url = $"{ESUMMARY_ENDPOINT}" +
				  $"?db=pubmed" +
				  $"&id={idList}" +
				  $"&retmode=json";

		if (!string.IsNullOrEmpty(_apiKey))
			url += $"&api_key={_apiKey}";

		return url;
	}

	public string BuildEFetchXMLUrl(IEnumerable<string> ids)
	{
		if (ids == null || !ids.Any())
			throw new ArgumentException("IDs cannot be null or empty.", nameof(ids));

		var idList = string.Join(",", ids);
		var url = $"{EFETCH_ENDPOINT}" +
				  $"?db=pubmed" +
				  $"&id={idList}" +
				  $"&retmode=xml";

		if (!string.IsNullOrEmpty(_apiKey))
			url += $"&api_key={_apiKey}";

		return url;
	}
}