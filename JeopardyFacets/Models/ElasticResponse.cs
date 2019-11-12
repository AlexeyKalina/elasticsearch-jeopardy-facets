using System.Collections.Generic;

namespace JeopardyFacets.Models
{
	public class ElasticResponse
	{
		public List<JeopadyQuestion> Questions { get; set; }
		public List<Facet> Facets { get; set; }
		public long Count { get; set; }
	}
}
