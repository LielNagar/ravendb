namespace Raven.Client.ServerWide
{
    internal static class Helpers
    {
        public static string ClusterStateMachineValuesPrefix(string databaseName)
        {
            return $"values/{databaseName}/";
        }
    }
}
