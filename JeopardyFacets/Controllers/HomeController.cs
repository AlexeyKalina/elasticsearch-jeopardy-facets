using System.Linq;
using JeopardyFacets.Models;
using JeopardyFacets.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace JeopardyFacets.Controllers
{
	public class HomeController : Controller
	{
		private const string ElasticUrl = ""; // use real url to elastic host
		private readonly ElasticJeopardyClient _elasticJeopardyClient = new ElasticJeopardyClient(ElasticUrl);
		public ViewResult Index()
		{
			var response = _elasticJeopardyClient.Search("");
			var search = new SearchViewModel
			{
				Facets = _elasticJeopardyClient.GetAllFacets(),
				Questions = response.Questions,
				Count = response.Count
			};
			search.Facets.ForEach(facet =>
			{
				if (facet.IsValue())
					facet.Values.Sort((x, y) => int.Parse(x.Key).CompareTo(int.Parse(y.Key)));
			});

			return View(search);
		}

		[HttpPost]
		public IActionResult Index(SearchViewModel searchViewModel)
		{
			var response = _elasticJeopardyClient.Search(searchViewModel.Input, searchViewModel.Facets);
			searchViewModel.Questions = response.Questions;

			if (response.Facets != null)
			{
				searchViewModel.Facets.ForEach(facet =>
				{
					facet.Values.ForEach(val =>
					{
						val.IsShow = response.Facets.Any(f => f.Values.Any(v => v.Key == val.Key));
						val.Count = response.Facets.First(f => f.Name == facet.Name).Values.FirstOrDefault(v => v.Key == val.Key)?.Count;
					});
					if (facet.IsValue())
						facet.Values.Sort((x, y) => int.Parse(x.Key).CompareTo(int.Parse(y.Key)));
				});
			}

			searchViewModel.Count = response.Count;
			return View(searchViewModel);
		}
	}
}