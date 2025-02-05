using Raven.Client.Documents.Operations;

namespace Raven.Client.Documents.Changes
{
    public interface IDatabaseChanges :
            IDocumentChanges<DocumentChange>,
            IIndexChanges<IndexChange>,
            IOperationChanges<OperationStatusChange>,
            ICounterChanges<CounterChange>,
            ITimeSeriesChanges<TimeSeriesChange>,
            IConnectableChanges<IDatabaseChanges>
    {
    }
}
