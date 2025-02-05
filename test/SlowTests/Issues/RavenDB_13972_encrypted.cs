﻿using System.Threading.Tasks;
using Raven.Server.Config;
using Raven.Server.Utils.Enumerators;
using Xunit;
using Xunit.Abstractions;

namespace SlowTests.Issues
{
    public class RavenDB_13972_encrypted : RavenDB_13972
    {
        private const int _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded = PulsedEnumerationState<object>.DefaultNumberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded;

        public RavenDB_13972_encrypted(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(2 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 10, 2, 2, 2 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 10, 2)]
        [InlineData(2 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 10, 2, 2, 0, 2)]
        [InlineData(2 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 10, 2, 2, 2 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 10, 3)]
        [InlineData(4 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 3, 2, 2, 0, 3)]
        [InlineData(4 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 3, 2, 2, 4 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 3, 2)]
        public async Task CanExportWithPulsatingReadTransaction(int numberOfUsers, int numberOfCountersPerUser, int numberOfRevisionsPerDocument, int numberOfOrders, int deleteUserFactor)
        {
            var file = GetTempFileName();
            var fileAfterDeletions = GetTempFileName();
            var result = await Encryption.EncryptedServerAsync();

            using (var storeToExport = GetDocumentStore(new Options
            {
                AdminCertificate = result.Certificates.ServerCertificate.Value,
                ClientCertificate = result.Certificates.ServerCertificate.Value,
                ModifyDatabaseName = s => result.DatabaseName,
                ModifyDatabaseRecord = record =>
                {
                    record.Settings[RavenConfiguration.GetKey(x => x.Databases.PulseReadTransactionLimit)] = "0";
                    record.Encrypted = true;
                }
            }))
            using (var server = GetNewServer(new ServerCreationOptions()))
            using (var storeToImport = GetDocumentStore(new Options
            {
                Server = server
            }))
            using (var storeToAfterDeletions = GetDocumentStore(new Options
            {
                Server = server
            }))
            {
                await CanExportWithPulsatingReadTransaction_ActualTest(numberOfUsers, numberOfCountersPerUser, numberOfRevisionsPerDocument, numberOfOrders, deleteUserFactor, storeToExport, file, storeToImport, fileAfterDeletions, storeToAfterDeletions);
            }
        }

        [Theory]
        [InlineData(2 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 10, 0, 2)]
        [InlineData(2 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 10, 2 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 10, 3)]
        [InlineData(4 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 3, 0, 3)]
        [InlineData(4 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 3, 4 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 3, 2)]
        public async Task CanStreamDocumentsWithPulsatingReadTransaction(int numberOfUsers, int numberOfOrders, int deleteUserFactor)
        {
            var result = await Encryption.EncryptedServerAsync();

            using (var store = GetDocumentStore(new Options
            {
                AdminCertificate = result.Certificates.ServerCertificate.Value,
                ClientCertificate = result.Certificates.ServerCertificate.Value,
                ModifyDatabaseName = s => result.DatabaseName,
                ModifyDatabaseRecord = record =>
                {
                    record.Settings[RavenConfiguration.GetKey(x => x.Databases.PulseReadTransactionLimit)] = "0";
                    record.Encrypted = true;
                }
            }))
            {
                CanStreamDocumentsWithPulsatingReadTransaction_ActualTest(numberOfUsers, numberOfOrders, deleteUserFactor, store);
            }
        }

        [Theory]
        [InlineData(2 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 10)]
        [InlineData(4 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 3)]
        [InlineData(10 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 3)]
        public async Task CanStreamQueryWithPulsatingReadTransaction(int numberOfUsers)
        {
            var result = await Encryption.EncryptedServerAsync();

            using (var store = GetDocumentStore(new Options
            {
                AdminCertificate = result.Certificates.ServerCertificate.Value,
                ClientCertificate = result.Certificates.ServerCertificate.Value,
                ModifyDatabaseName = s => result.DatabaseName,
                ModifyDatabaseRecord = record =>
                {
                    record.Settings[RavenConfiguration.GetKey(x => x.Databases.PulseReadTransactionLimit)] = "0";
                    record.Encrypted = true;
                }
            }))
            {
                await CanStreamQueryWithPulsatingReadTransaction_ActualTestAsync(numberOfUsers, store);
            }
        }

        [Theory]
        [InlineData(2 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 10)]
        [InlineData(4 * _numberOfEnumeratedDocumentsToCheckIfPulseLimitExceeded + 3)]
        public async Task CanStreamCollectionQueryWithPulsatingReadTransaction(int numberOfUsers)
        {
            var result = await Encryption.EncryptedServerAsync();

            using (var store = GetDocumentStore(new Options
            {
                AdminCertificate = result.Certificates.ServerCertificate.Value,
                ClientCertificate = result.Certificates.ServerCertificate.Value,
                ModifyDatabaseName = s => result.DatabaseName,
                ModifyDatabaseRecord = record =>
                {
                    record.Settings[RavenConfiguration.GetKey(x => x.Databases.PulseReadTransactionLimit)] = "0";
                    record.Encrypted = true;
                }
            }))
            {
                await CanStreamCollectionQueryWithPulsatingReadTransaction_ActualTestAsync(numberOfUsers, store);
            }
        }
    }
}
