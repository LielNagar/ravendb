﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Raven.Client.Documents.Conventions;
using Raven.Client.Http;
using Raven.Client.Json.Converters;
using Sparrow;
using Sparrow.Extensions;
using Sparrow.Json;

namespace Raven.Client.Documents.Operations.TimeSeries
{
    public class GetTimeSeriesOperation : IOperation<TimeSeriesDetails>
    {
        private readonly string _docId;
        private readonly IEnumerable<TimeSeriesRange> _ranges;
        private readonly int _start;
        private readonly int _pageSize;

        public GetTimeSeriesOperation(string docId, string timeseries, DateTime from, DateTime to, int start = 0, int pageSize = int.MaxValue)
            : this(docId, start, pageSize)
        {
            _ranges = new List<TimeSeriesRange>
            {
                new TimeSeriesRange
                {
                    Name = timeseries,
                    From = from,
                    To = to
                }
            };
        }

        public GetTimeSeriesOperation(string docId, IEnumerable<TimeSeriesRange> ranges, int start = 0, int pageSize = int.MaxValue) 
            : this(docId, start, pageSize)
        {
            _ranges = ranges ?? throw new ArgumentNullException(nameof(ranges));
        }

        private GetTimeSeriesOperation(string docId, int start, int pageSize)
        {
            if (string.IsNullOrEmpty(docId))
                throw new ArgumentNullException(nameof(docId));

            _docId = docId;
            _start = start;
            _pageSize = pageSize;
        }

        public RavenCommand<TimeSeriesDetails> GetCommand(IDocumentStore store, DocumentConventions conventions, JsonOperationContext context, HttpCache cache)
        {
            return new GetTimeSeriesCommand(_docId, _ranges, _start, _pageSize);
        }

        internal class GetTimeSeriesCommand : RavenCommand<TimeSeriesDetails>
        {
            private readonly string _docId;
            private readonly IEnumerable<TimeSeriesRange> _ranges;
            private readonly int _start;
            private readonly int _pageSize;

            public GetTimeSeriesCommand(string docId, IEnumerable<TimeSeriesRange> ranges, int start, int pageSize)
            {
                _docId = docId ?? throw new ArgumentNullException(nameof(docId));
                _ranges = ranges;
                _start = start;
                _pageSize = pageSize;
            }

            public override HttpRequestMessage CreateRequest(JsonOperationContext ctx, ServerNode node, out string url)
            {
                var pathBuilder = new StringBuilder(node.Url);
                pathBuilder.Append("/databases/")
                    .Append(node.Database)
                    .Append("/timeseries")
                    .Append("?docId=")
                    .Append(Uri.EscapeDataString(_docId));

                if (_start > 0)
                {
                    pathBuilder.Append("&start=")
                        .Append(_start);
                }

                if (_pageSize < int.MaxValue)
                {
                    pathBuilder.Append("&pageSize=")
                        .Append(_pageSize);
                }

                foreach (var range in _ranges)
                {
                    pathBuilder.Append("&name=")
                        .Append(range.Name)
                        .Append("&from=")
                        .Append(range.From.EnsureUtc().GetDefaultRavenFormat())
                        .Append("&to=")
                        .Append(range.To.EnsureUtc().GetDefaultRavenFormat());
                }

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };

                url = pathBuilder.ToString();

                return request;
            }

            public override void SetResponse(JsonOperationContext context, BlittableJsonReaderObject response, bool fromCache)
            {
                if (response == null)
                    return;

                Result = JsonDeserializationClient.TimeSeriesDetails(response);
            }

            public override bool IsReadRequest => true;
        }
    }

    public class TimeSeriesDetails
    {
        public string Id;
        public Dictionary<string, List<TimeSeriesRangeResult>> Values;
    }
}
