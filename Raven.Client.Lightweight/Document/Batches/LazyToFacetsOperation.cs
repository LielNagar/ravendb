﻿using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Data;
using Raven.Abstractions.Extensions;
using Raven.Client.Shard;
using Raven.Json.Linq;

namespace Raven.Client.Document.Batches
{
	public class LazyToFacetsOperation : ILazyOperation
	{
		private readonly string index;
		private readonly string facetSetupDoc;
		private readonly IndexQuery query;

		public LazyToFacetsOperation(string index, string facetSetupDoc, IndexQuery query)
		{
			this.index = index;
			this.facetSetupDoc = facetSetupDoc;
			this.query = query;
		}

		public GetRequest CraeteRequest()
		{
			return new GetRequest
			{
				Url = "/facets/" + index,
				Query = string.Format("facetDoc={0}&query={1}",
				                      facetSetupDoc,
				                      query.Query)
			};
		}

		public object Result { get; private set; }
		public bool RequiresRetry { get; private set; }

		public void HandleResponse(GetResponse response)
		{
			if (response.Status != 200)
			{
				throw new InvalidOperationException("Got an unexpected response code for the request: " + response.Status + "\r\n" +
				                                    response.Result);
			}

			var result = RavenJObject.Parse(response.Result);
			Result = result.JsonDeserialization<IDictionary<string, IEnumerable<FacetValue>>>();
		}

		public void HandleResponses(GetResponse[] responses, ShardStrategy shardStrategy)
		{
			var result = new Dictionary<string, IEnumerable<FacetValue>>();

			IEnumerable<IGrouping<string, KeyValuePair<string, IEnumerable<FacetValue>>>> list = responses.Select(response => RavenJObject.Parse(response.Result))
				.SelectMany(jsonResult => jsonResult.JsonDeserialization<IDictionary<string, IEnumerable<FacetValue>>>())
				.GroupBy(x => x.Key);

			foreach (var facet in list)
			{
				var individualFacet = facet.SelectMany(x=>x.Value).GroupBy(x=>x.Range)
					.Select(g=>new FacetValue
					{
						Count = g.Sum(y=>y.Count),
						Range = g.Key
					});
				result[facet.Key] = individualFacet.ToList();
			}

			Result = result;
		}

		public IDisposable EnterContext()
		{
			return null;
		}
	}
}
