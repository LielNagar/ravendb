﻿//-----------------------------------------------------------------------
// <copyright file="IDocumentSession.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace Raven.Client.Documents.Session
{
    /// <summary>
    /// Provides an access to DocumentSession TimeSeries API.
    /// </summary>
    public partial interface IDocumentSession
    {
        /// <inheritdoc cref="IAsyncDocumentSession.TimeSeriesFor"/>
        ISessionDocumentTimeSeries TimeSeriesFor(string documentId, string name);

        /// <inheritdoc cref="IAsyncDocumentSession.TimeSeriesFor(object, string)"/>
        ISessionDocumentTimeSeries TimeSeriesFor(object entity, string name);

        /// <inheritdoc cref="IAsyncDocumentSession.TimeSeriesFor{TValues}(object, string)"/>
        ISessionDocumentTypedTimeSeries<TValues> TimeSeriesFor<TValues>(object entity, string name = null) where TValues : new();

        /// <inheritdoc cref="IAsyncDocumentSession.TimeSeriesFor{TValues}"/>
        ISessionDocumentTypedTimeSeries<TValues> TimeSeriesFor<TValues>(string documentId, string name = null) where TValues : new();

        /// <inheritdoc cref="IAsyncDocumentSession.TimeSeriesRollupFor{TValues}"/>
        ISessionDocumentRollupTypedTimeSeries<TValues> TimeSeriesRollupFor<TValues>(object entity, string policy, string raw = null) where TValues : new();

        /// <inheritdoc cref="IAsyncDocumentSession.TimeSeriesRollupFor{TValues}(string, string, string)"/>
        ISessionDocumentRollupTypedTimeSeries<TValues> TimeSeriesRollupFor<TValues>(string documentId, string policy, string raw = null) where TValues : new();

        /// <inheritdoc cref="IAsyncDocumentSession.IncrementalTimeSeriesFor"/>
        ISessionDocumentIncrementalTimeSeries IncrementalTimeSeriesFor(string documentId, string name);

        /// <inheritdoc cref="IAsyncDocumentSession.IncrementalTimeSeriesFor(object, string)"/>
        ISessionDocumentIncrementalTimeSeries IncrementalTimeSeriesFor(object entity, string name);

        /// <inheritdoc cref="IAsyncDocumentSession.IncrementalTimeSeriesFor{TValues}(object, string)"/>
        ISessionDocumentTypedIncrementalTimeSeries<TValues> IncrementalTimeSeriesFor<TValues>(object entity, string name = null) where TValues : new();

        /// <inheritdoc cref="IAsyncDocumentSession.IncrementalTimeSeriesFor{TValues}"/>
        ISessionDocumentTypedIncrementalTimeSeries<TValues> IncrementalTimeSeriesFor<TValues>(string documentId, string name = null) where TValues : new();
    }
}
