using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Newtonsoft.Json.Linq;

namespace JeopardyFacets.Models
{
	public class ElasticJeopardyClient
	{
		private const String IndexName = "jeopardy";
		private const String TypeName = "question";
		private readonly ElasticClient _client;

		public ElasticJeopardyClient(string uri)
		{
			_client = new ElasticClient(
				new ConnectionSettings(
					new Uri(uri)).DefaultIndex(IndexName).DisableDirectStreaming());
		}

		public ElasticResponse Search(string query, List<Facet> facets = null)
		{
			SearchRequest<dynamic> searchRequest;
			if (facets == null)
			{
				searchRequest = new SearchRequest<dynamic>(IndexName, TypeName)
				{
					Query = new BoolQuery
					{
						Should = new List<QueryContainer>
						{
							new MatchQuery
							{
								Field = "question",
								Query = query,
								Operator = Operator.And
							}
						}
					},
					Size = 20
				};
			}
			else
			{
				var filter = new List<QueryContainer>();
				foreach (var facet in facets)
				{
					filter.Add(new TermsQuery
					{
						Field = facet.Name,
						Terms = facet.Values.Where(x => x.IsUse).Select(x => x.Key)
					});
				}
				searchRequest = new SearchRequest<dynamic>(IndexName, TypeName)
				{
					Query = new BoolQuery
					{
						Must = new List<QueryContainer>
						{
							new MatchQuery
							{
								Field = "question",
								Query = query,
								Operator = Operator.And
							}
						},
						Filter = filter
					},
					Aggregations = new AggregationDictionary
					{
						{ "round", new AggregationContainer
							{
								Terms = new TermsAggregation("round")
								{
									Field = "round",
									Size = 20
								}
							}
						},
						{ "value_aggs", new AggregationContainer
							{
								Terms = new TermsAggregation("value_aggs")
								{
									Field = "value",
									Size = 100
								}
							}
						}
					},
					Size = 20
				};
			}

			var searchResponse = _client.Search<dynamic>(searchRequest);

			var results = new List<JeopadyQuestion>();

			if (searchResponse.Hits.Count > 0)
			{
				foreach (var hit in searchResponse.Hits)
				{
					JObject res = JObject.Parse(hit.Source.ToString());

					results.Add(new JeopadyQuestion
					{
						Answer = res["answer"].ToString(),
						Question = res["question"].ToString(),
						Category = res["category"].ToString()
					});
				}
			}


			if (searchResponse.Aggs != null && facets != null)
			{
				var aggs = new List<Facet>();
				aggs.Add(GetFacetByName("round", "round", searchResponse));
				aggs.Add(GetFacetByName("value_aggs", "value", searchResponse));

				return new ElasticResponse
				{
					Facets = aggs,
					Questions = results,
					Count = searchResponse.Total
				};
			}

			return new ElasticResponse
			{
				Questions = results,
				Count = searchResponse.Total
			};
		}

		public List<Facet> GetAllFacets()
		{
			var aggsRequest = new SearchRequest<dynamic>(IndexName, TypeName)
			{
				Aggregations = new AggregationDictionary
				{
					{ "round", new AggregationContainer
						{
							Terms = new TermsAggregation("round")
							{
								Field = "round",
								Size = 20
							}
						}
					},
					{ "value_aggs", new AggregationContainer
						{
							Terms = new TermsAggregation("value_aggs")
							{
								Field = "value",
								Size = 100
							}
						}
					}
				}
			};

			var aggsResponse = _client.Search<dynamic>(aggsRequest);

			var results = new List<Facet>();

			if (aggsResponse.Aggs != null)
			{

				results.Add(GetFacetByName("round", "round", aggsResponse));
				results.Add(GetFacetByName("value_aggs", "value", aggsResponse));
			}

			return results;

		}

		private static Facet GetFacetByName(string aggsName, string name, ISearchResponse<dynamic> aggsResponse)
		{
			var facet = new Facet
			{
				Name = name
			};
			var values = new List<FacetElement>();
			foreach (var bucket in aggsResponse.Aggs.Terms(aggsName).Buckets)
			{
				values.Add(new FacetElement
				{
					Key = bucket.Key,
					Count = bucket.DocCount,
					IsShow = true
				});
			}
			facet.Values = values;
			return facet;
		}
	}
}
