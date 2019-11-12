using System.Collections.Generic;

namespace JeopardyFacets.Models
{
	public class Facet
	{
		public string Name { get; set; }
		public List<FacetElement> Values { get; set; }

		public bool IsValue()
		{
			return Name == "value";
		}
	}

	public class FacetElement
	{
		public string Key { get; set; }
		public long? Count { get; set; }
		public bool IsUse { get; set; }
		public bool IsShow { get; set; }
	}
}
