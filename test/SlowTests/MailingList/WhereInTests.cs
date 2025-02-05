﻿using System.Collections.Generic;
using System.Linq;
using FastTests;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Server.Documents.Indexes.Persistence.Lucene.Analyzers;
using Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SlowTests.MailingList
{
    public class WhereInTests : RavenTestBase
    {
        public WhereInTests(ITestOutputHelper output) : base(output)
        {
        }

        [RavenTheory(RavenTestCategory.Querying)]
        [RavenData(SearchEngineMode = RavenSearchEngineMode.All, DatabaseMode = RavenDatabaseMode.Single)]
        public void WhereIn_using_index_notAnalyzed(Options options)
        {
            using (IDocumentStore documentStore = GetDocumentStore(options))
            {
                new PersonsNotAnalyzed().Execute(documentStore);

                string[] names = { "Person One", "PersonTwo" };

                StoreObjects(new List<Person>
                {
                    new Person {Name = names[0]},
                    new Person {Name = names[1]}
                }, documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var query = session.Advanced.DocumentQuery<Person, PersonsNotAnalyzed>().WhereIn(p => p.Name, names, exact: true);
                    Assert.Equal(2, query.ToList().Count());
                }
            }
        }

        [Fact]
        public void SameHash()
        {
            var perFieldAnalyzerComparer = new LuceneRavenPerFieldAnalyzerWrapper.PerFieldAnalyzerComparer();
            Assert.Equal(perFieldAnalyzerComparer.GetHashCode("Name"), perFieldAnalyzerComparer.GetHashCode("@in<Name>"));
            Assert.True(perFieldAnalyzerComparer.Equals("Name", "@in<Name>"));
        }
        
        [RavenTheory(RavenTestCategory.Querying)]
        [RavenData(SearchEngineMode = RavenSearchEngineMode.All, DatabaseMode = RavenDatabaseMode.Single)]
        public void WhereIn_using_index_analyzed(Options options)
        {
            using (IDocumentStore documentStore = GetDocumentStore(options))
            {
                new PersonsAnalyzed().Execute(documentStore);

                string[] names = { "Person One", "PersonTwo" };

                StoreObjects(new List<Person>
                {
                    new Person {Name = names[0]},
                    new Person {Name = names[1]}
                }, documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var query = session.Advanced.DocumentQuery<Person, PersonsAnalyzed>().Search(p => p.Name, string.Join(" ", names));
                    Assert.Equal(2, query.ToList().Count());
                }
            }
        }

        [RavenTheory(RavenTestCategory.Querying)]
        [RavenData(SearchEngineMode = RavenSearchEngineMode.All, DatabaseMode = RavenDatabaseMode.Single)]
        public void WhereIn_not_using_index(Options options)
        {
            using (IDocumentStore documentStore = GetDocumentStore(options))
            {

                string[] names = { "Person One", "PersonTwo" };

                StoreObjects(new List<Person>
                {
                    new Person {Name = names[0]},
                    new Person {Name = names[1]}
                }, documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var query = session.Advanced.DocumentQuery<Person>().WhereIn(p => p.Name, names);
                    Assert.Equal(2, query.ToList().Count());
                }
            }
        }

        [RavenTheory(RavenTestCategory.Querying)]
        [RavenData(SearchEngineMode = RavenSearchEngineMode.All, DatabaseMode = RavenDatabaseMode.Single)]
        public void Where_In_using_query_index_notAnalyzed(Options options)
        {
            using (IDocumentStore documentStore = GetDocumentStore(options))
            {
                new PersonsNotAnalyzed().Execute(documentStore);

                string[] names = { "Person One", "PersonTwo" };

                StoreObjects(new List<Person>
                {
                    new Person {Name = names[0]},
                    new Person {Name = names[1]}
                }, documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var query = session.Query<Person, PersonsNotAnalyzed>().Where(p => p.Name.In(names), exact: true);
                    Assert.Equal(2, query.Count());
                }
            }
        }

        [RavenTheory(RavenTestCategory.Querying)]
        [RavenData(SearchEngineMode = RavenSearchEngineMode.All, DatabaseMode = RavenDatabaseMode.Single)]
        public void Where_In_using_query_index_analyzed(Options options)
        {
            using (IDocumentStore documentStore = GetDocumentStore(options))
            {
                new PersonsAnalyzed().Execute(documentStore);

                string[] names = { "Person One", "PersonTwo" };

                StoreObjects(new List<Person>
                {
                    new Person {Name = names[0]},
                    new Person {Name = names[1]}
                }, documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var query = session.Query<Person, PersonsAnalyzed>().Search(p => p.Name, string.Join(" ", names));
                    Assert.Equal(2, query.Count());
                }
            }
        }

        [RavenTheory(RavenTestCategory.Querying)]
        [RavenData(SearchEngineMode = RavenSearchEngineMode.All, DatabaseMode = RavenDatabaseMode.All)]
        public void Where_In_using_query(Options options)
        {
            using (IDocumentStore documentStore = GetDocumentStore(options))
            {
                string[] names = { "Person One", "PersonTwo" };

                StoreObjects(new List<Person>
                {
                    new Person {Name = names[0]},
                    new Person {Name = names[1]}
                }, documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var query = session.Query<Person>().Where(p => p.Name.In(names));
                    Assert.Equal(2, query.Count());
                }
            }
        }

        private void StoreObjects<T>(IEnumerable<T> objects, IDocumentStore documentStore)
        {
            using (var session = documentStore.OpenSession())
            {
                foreach (var o in objects)
                {
                    session.Store(o);
                }
                session.SaveChanges();
            }
            Indexes.WaitForIndexing(documentStore);
        }

        private class Person
        {
            public string Name { get; set; }
        }

        private class PersonsNotAnalyzed : AbstractIndexCreationTask<Person>
        {
            public PersonsNotAnalyzed()
            {
                Map = organizations => from o in organizations
                                       select new { o.Name };

                Indexes.Add(x => x.Name, FieldIndexing.Exact);
            }
        }

        private class PersonsAnalyzed : AbstractIndexCreationTask<Person>
        {
            public PersonsAnalyzed()
            {
                Map = organizations => from o in organizations
                                       select new { o.Name };

                Indexes.Add(x => x.Name, FieldIndexing.Search);
            }
        }
    }
}
