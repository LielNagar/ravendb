﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FastTests;
using Raven.Client.Documents.Subscriptions;
using Sparrow.Server;
using Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SlowTests.Client.Subscriptions
{
    public class RavenDB_15553:RavenTestBase
    {
        public RavenDB_15553(ITestOutputHelper output) : base(output)
        {
        }

        private readonly TimeSpan _reasonableWaitTime = TimeSpan.FromSeconds(10);

        [RavenTheory(RavenTestCategory.Subscriptions)]
        [RavenData(DatabaseMode = RavenDatabaseMode.All)]
        public async Task ShouldUseIdPropertySupportWhenTranslatingPredicateToJS(Options options)
        {
            var workspaceId = "workspaces/1";

            using (var store = GetDocumentStore(options))
            {
                using (var session = store.OpenAsyncSession())
                {
                    await session.StoreAsync(new Workspace
                    {
                        Teams = new List<string>
                        {
                            "teams/1", "teams/8", "teams/11"
                        }
                    }, workspaceId);

                    await session.SaveChangesAsync();
                }

                using (var session = store.OpenAsyncSession())
                {
                    var workspace = await session.LoadAsync<Workspace>(workspaceId);
                    var subscriptionName = await store.Subscriptions.CreateAsync(new SubscriptionCreationOptions<Team>
                    {
                        Filter = doc => workspace.Teams.Contains(doc.Id),
                        Projection = doc => new
                        {
                            Id = doc.Id,
                            Name = doc.Name
                        }
                    });

                    var subscription = store.Subscriptions.GetSubscriptionWorker<Team>(subscriptionName);

                    var mre = new AsyncManualResetEvent();
                    var names = new List<string>();

                    await session.StoreAsync(new Team
                    {
                        Name = "R&D"
                    }, "teams/1");
                    await session.StoreAsync(new Team
                    {
                        Name = "Sales"
                    }, "teams/2");
                    await session.StoreAsync(new Team
                    {
                        Name = "Ops"
                    }, "teams/3");
                    await session.StoreAsync(new Team
                    {
                        Name = "Support"
                    }, "teams/8");
                    await session.StoreAsync(new Team
                    {
                        Name = "Marketing"
                    }, "teams/11");

                    await session.SaveChangesAsync();

                    var count = 0;
                    _ = subscription.Run(x =>
                    {
                        names.AddRange(x.Items.Select(i => i.Result.Name).ToList());
                        count+= x.Items.Count;
                        if(count >= 3)
                            mre.Set();
                    });

                    Assert.True(await mre.WaitAsync(_reasonableWaitTime));
                    names.Sort();
                    Assert.Equal(new[] { "Marketing", "R&D", "Support" }, names);
                }
            }
        }

        private class Workspace
        {
            public List<string> Teams { get; set; }
        }

        private class Team
        {
            public string Id { get; set; }

            public string Name { get; set; }

        }
    }
}
