using System.Collections.Generic;

namespace JeopardyFacets.Models.ViewModels
{
	public class SearchViewModel
	{
		public string Input { get; set; }
		public List<JeopadyQuestion> Questions { get; set; }
		public List<Facet> Facets { get; set; }
		public long Count { get; set; }
	}
}
