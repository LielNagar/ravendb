﻿using System.Linq;
using Raven.Client.Documents.Indexes;
using Raven.Tests.Core.Utils.Entities;
using Tests.Infrastructure;
using Xunit.Abstractions;

namespace FastTests.Issues
{
    public class RavenDB_11791 : RavenTestBase
    {
        public RavenDB_11791(ITestOutputHelper output) : base(output)
        {
        }


        public class PeopleIndex : AbstractIndexCreationTask<Person>
        {
            public PeopleIndex()
            {
                Map = people => from person in people
                                select new
                                {
                                    person.Name
                                };
            }
        }
        
        [RavenTheory(RavenTestCategory.Indexes)]
        [RavenData(SearchEngineMode = RavenSearchEngineMode.All)]
        public void Test(Options options)
        {
            using (var store = GetDocumentStore(options))
            {
                new PeopleIndex().Execute(store);

                using (var session = store.OpenSession())
                {
                    session.Store(new Person(), "people/1");
                    session.SaveChanges();
                }
                
                Indexes.WaitForIndexing(store);


                using (var session = store.OpenSession())
                {
                    session.Delete("people/1");
                    session.SaveChanges();
                }
                Indexes.WaitForIndexing(store);
                
                using (var session = store.OpenSession())
                {
                    session.Store(new Person());
                    session.SaveChanges();
                }

            }

        }
    }
}
