namespace ResearchPublicationTracker.Models
{
	public class DataTableRequest
	{
		public int Draw { get; set; }
		public int Start { get; set; }
		public int Length { get; set; }
		public string SearchTerm { get; set; } = null!;
		public DataTableSearch Search { get; set; } = null!;
		public List<DataTableColumn> Columns { get; set; } = null!;
		public List<DataTableOrder> Order { get; set; } = null!;
	}

	public class DataTableOrder
	{
		public int Column { get; set; }
		public string Dir { get; set; } = null!;
	}

	public class DataTableColumn
	{
		public string Data { get; set; } = null!;
		public string Name { get; set; } = null!;
		public bool Searchable { get; set; }
		public bool Orderable { get; set; }
		public DataTableSearch Search { get; set; } = null!;
	}

	public class DataTableSearch
	{
		public string Value { get; set; } = null!;
		public bool Regex { get; set; }
	}
}
