using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacetedSearch.Models;
using Elasticsearch.Net;
using Nest;
using Nest.DSL;
using System.Web.Configuration;

namespace FacetedSearch.BusinessLogic
{
    //    Komplizierte Abfragen (evtl. ohne DSL-Unterstützung) ==> JSON-Objekt direkt konvertieren (nützlich für Beispiele ohne NEST)
    //    ===========================================================================================================================
    //    var query = "blabla";
    //    var q = new
    //        {
    //            query = new
    //            {
    //                text = new
    //                {
    //                    _all= query
    //                }
    //            }, 
    //            from = (page-1)*pageSize, 
    //            size=pageSize
    //        };
    //        var qJson = JsonConvert.SerializeObject(q);
    //        var hits = _elasticClient.Search<SearchItem>(qJson);

    public class SearchManager
    {
        /// <summary>
        /// our reference to the elasticsearch engine
        /// </summary>
        private static string ConnectionString = WebConfigurationManager.ConnectionStrings["Elasticsearch"].ConnectionString;
        
        /// <summary>
        /// Like a database name
        /// </summary>
        private static string IndexName = "search-demo";

        /// <summary>
        /// Defaults for paging
        /// </summary>
        private static int pageSize = 20;

        /// <summary>
        /// How many results should a query return max?
        /// </summary>
        private static int maxResults = 400;


        public static bool IndexExists()
        {
            var client = GetClient();

            // if you end up here: check whether elasticsearch is up and running on port 9200 :-)
            //
            return client.IndexExists(IndexName).Exists;
        }

        /// <summary>
        /// Delete and recreate index - create mappings for special fields
        /// </summary>
        public static void ResetIndex()
        {

            var client = GetClient();
           
            client.DeleteIndex(IndexName);

            client.CreateIndex(IndexName);

            // treat EMail as a word (do not strip to single fields: ralph@bla.com -- would be in the index --> ralph bla com) 
            //
            client.Map<BlogAuthor>(u => u.Properties(p => p.String(n => n.Name(name => name.EMail).Index(FieldIndexOption.NotAnalyzed))));

        }

        /// <summary>
        /// Add or update article in search index
        /// </summary>
        /// <param name="bke"></param>
        public static void Index(BlogArticle ar)
        {
            var index = GetClient().Index(ar);
        }

        /// <summary>
        /// Add or update author in search index
        /// </summary>
        /// <param name="bke"></param>
        public static void Index(BlogAuthor au)
        {

            var index = GetClient().Index(au);
        }


        // Basis für Http-Verbindung (REST)
        //
        private static ElasticClient GetClient()
        {
            var node = new Uri(ConnectionString);

            var settings = new ConnectionSettings(
                node,
                defaultIndex: IndexName
            );

            return new ElasticClient(settings);
        }



        public static SearchResultViewModel SearchArticle(string SearchTerm, int page, Dictionary<string, string> filters, int _pageSize, int _maxResults)
        {
            Dictionary<string, string> fieldMappings = new Dictionary<string, string>()
            {
                { "language", "languageCode"},
            };


            List<FilterContainer> termFilters = new List<FilterContainer>();
            foreach (string key in filters.Keys)
            {
                if (!fieldMappings.ContainsKey(key))
                {
                    continue;
                }


                termFilters.Add(new TermFilter()
                {
                    Field = fieldMappings[key],
                    Value = filters[key]
                });

            }

            // if there are more than one word --> concat using AND 
            //
            string[] splitSearch = SearchTerm.Split(' ');

            List<QueryContainer> searchQueryList = new List<QueryContainer>();
            foreach (var strText in splitSearch)
            {
                searchQueryList.Add(Query<BlogArticle>.Prefix("_all", strText)); // prefix finds incomplete words - use .Term for an exact match
            }


            var results = GetClient().Search<BlogArticle>(s => s

                                             .Aggregations(a => a
                                               .Terms("language", o => o.Field(f => f.LanguageCode).Size(10))
                                              )
                                             .Query(q => q
                                                 .Filtered(fq => fq
                                                    .Query(sq => sq.Bool(b => b.Must(searchQueryList.ToArray())))
                                                    .Filter(f => f.And(termFilters.ToArray()))

                                                 )
                                             )
                                             .Highlight(h => h.PreTags("<span class='highlight'>")
                                             .PostTags("</span>")
                                             .OnFields(new Action<HighlightFieldDescriptor<BlogArticle>>[] { _ => _.OnField(c => c.Teaser).FragmentSize(100).NumberOfFragments(1) })
                                             ).Size(_maxResults).Skip(page * _pageSize).Take(_pageSize)
                                           );
          

            return new SearchResultViewModel() { Result = results, SearchTerm = SearchTerm };
        }

        ///// <summary>
        ///// Gefilterte Suche durchführen
        ///// </summary>
        ///// <param name="SearchTerm"></param>
        ///// <returns></returns>
        //public static SearchResultData SearchEntry(string SearchTerm, int page, Dictionary<string, string> filters)
        //{
        //    return SearchEntry(SearchTerm, page, filters, pageSize, maxResults);
        //}


        //public static SearchResultData SearchEntry(string SearchTerm, int page)
        //{
        //    return SearchEntry(SearchTerm, page, pageSize, maxResults);
        //}

        ///// <summary>
        ///// Suche durchführen
        ///// </summary>
        ///// <param name="SearchTerm"></param>
        ///// <returns></returns>
        //public static SearchResultData SearchEntry(string SearchTerm, int page, int _pageSize, int _maxResults)
        //{
        //    // Suche nach mehreren Wörtern, die mit AND verknüpft werden
        //    //
        //    string[] splitSearch = SearchTerm.Split(' ');
        //    List<QueryContainer> searchQueryList = new List<QueryContainer>();

        //    foreach (var strText in splitSearch)
        //    {
        //        // var singleWordQuery = Query<BookletEntry>.Term("_all", strText);   // genauer Match
        //        var singlePrefixQuery = Query<BookletEntry>.Prefix("_all", strText);  // Anfang genügt!

        //        searchQueryList.Add(singlePrefixQuery);

        //    }
        //    var severalWordsQuery = Query<BookletEntry>.Bool(b => b.Must(searchQueryList.ToArray()));


        //    var results = GetClient().Search<BookletEntry>(s => s
        //                                      .Aggregations(a => a
        //                                        .Terms("booklet", o => o.Field(f => f.BookletId).Size(10))
        //                                        .Terms("creator", c => c.Field(f => f.UserCreated).Size(10))
        //                                        .Terms("company", c => c.Field(f => f.ExhibitorId).Size(10))
        //                                        .Terms("language", c => c.Field(f => f.Language).Size(10))
        //                                        .Terms("country", c => c.Field(f => f.Country).Size(10))
        //                                        .Terms("modifier", c => c.Field(f => f.UserLastModified).Size(10))
        //                                        .Terms("hall", c => c.Field(f => f.Hall).Size(10))
        //                                        .Terms("booth", c => c.Field(f => f.Booth).Size(10))
        //                                        .Terms("branches", c => c.Field(f => f.Branches).Size(10))
        //                                        .Terms("components", c => c.Field(f => f.Components).Size(10))
        //                                        .Terms("machinetype", c => c.Field(f => f.MachineType).Size(10))
        //                                        .Terms("status", c => c.Field(f => f.BookletEntryStatus).Size(10))
        //                                        .Terms("exhibitor", c => c.Field(f => f.ExhibitorId).Size(10))
        //                                      )
        //                                      .Query(severalWordsQuery)
        //                                      .Highlight(h => h.PreTags("<span class='highlight'>")
        //                                      .PostTags("</span>")
        //                                      .OnFields(new Action<HighlightFieldDescriptor<BookletEntry>>[] { _ => _.OnField(c => c.Content).FragmentSize(100).NumberOfFragments(1) })
        //                                      ).Size(_maxResults).Skip(page * _pageSize).Take(_pageSize)
        //                                   );

        //    return new SearchResultData() { Result = results, SearchTerm = SearchTerm };
        //}

        ///// <summary>
        ///// Suche durchführen in Ausstellern
        ///// </summary>
        ///// <param name="SearchTerm"></param>
        ///// <returns></returns>
        //public static SearchResultData SearchExhibitor(string SearchTerm, int page, Dictionary<string, string> filters)
        //{

        //    // Suche nach mehreren Wörtern, die mit AND verknüpft werden
        //    //
        //    string[] splitSearch = SearchTerm.Split(' ');
        //    List<QueryContainer> searchQueryList = new List<QueryContainer>();

        //    foreach (var strText in splitSearch)
        //    {
        //        // var singleWordQuery = Query<BookletEntry>.Term("_all", strText);   // genauer Match
        //        var singlePrefixQuery = Query<BookletEntry>.Prefix("_all", strText);  // Anfang genügt!

        //        searchQueryList.Add(singlePrefixQuery);

        //    }
        //    var severalWordsQuery = Query<BookletEntry>.Bool(b => b.Must(searchQueryList.ToArray()));

        //    ISearchResponse<Exhibitor> results = null;

        //    if (filters.Count == 0)
        //    {
        //        // a) Basisabfrage
        //        //
        //        results = GetClient().Search<Exhibitor>(s => s
        //                                          .Aggregations(a => a
        //                                            .Terms("country", c => c.Field(f => f.Country).Size(10))
        //                                          )
        //                                          .Query(severalWordsQuery)
        //                                          .Size(maxResults).Skip(page * pageSize).Take(pageSize)
        //                                       );
        //    }
        //    else
        //    {
        //        // b) Gefiltert
        //        //
        //        Dictionary<string, string> fieldMappings = new Dictionary<string, string>()
        //        {
        //            { "country", "country"}
        //        };

        //        List<FilterContainer> termFilters = new List<FilterContainer>();
        //        foreach (string key in filters.Keys)
        //        {
        //            termFilters.Add(new TermFilter()
        //            {
        //                Field = fieldMappings[key],
        //                Value = filters[key]
        //            });
        //        }

        //        results = GetClient().Search<Exhibitor>(s => s
        //                                     .Aggregations(a => a
        //                                       .Terms("country", c => c.Field(f => f.Country).Size(10))

        //                                     )
        //                                     .Query(q => q
        //                                         .Filtered(fq => fq
        //                                            .Query(sq => sq.Bool(b => b.Must(searchQueryList.ToArray())))
        //                                            .Filter(f => f.And(termFilters.ToArray()))
        //                                         )
        //                                     )
        //                                     .Size(maxResults).Skip(page * pageSize).Take(pageSize)
        //                                   );
        //    }
        //    return new SearchResultData() { ResultExhibitors = results, SearchTerm = SearchTerm };
        //}

        ///// <summary>
        ///// Suche durchführen in Benutzern
        ///// </summary>
        ///// <param name="SearchTerm"></param>
        ///// <param name="page"></param>
        ///// <param name="filters"></param>
        ///// <returns></returns>
        //public static SearchResultData SearchUser(string SearchTerm, int page, Dictionary<string, string> filters)
        //{


        //    // Suche nach mehreren Wörtern, die mit AND verknüpft werden
        //    //
        //    string[] splitSearch = SearchTerm.Split(' ');
        //    List<QueryContainer> searchQueryList = new List<QueryContainer>();


        //    foreach (var strText in splitSearch)
        //    {
        //        // var singleWordQuery = Query<BookletEntry>.Term("_all", strText);   // genauer Match
        //        List<QueryContainer> termOrPrefix = new List<QueryContainer>();
        //        termOrPrefix.Add(Query<BookletUser>.Term("eMail", strText));
        //        var singlePrefixQuery = Query<BookletUser>.Prefix("_all", strText);  // Anfang genügt!
        //        termOrPrefix.Add(singlePrefixQuery);


        //        searchQueryList.Add(Query<BookletUser>.Bool(b => b.Should(termOrPrefix.ToArray())));

        //    }

        //    var severalWordsQuery = Query<BookletUser>.Bool(b => b.Must(searchQueryList.ToArray()));

        //    ISearchResponse<BookletUser> results = null;

        //    if (filters.Count == 0)
        //    {
        //        // a) Basisabfrage
        //        //
        //        results = GetClient().Search<BookletUser>(s => s
        //                                          .Aggregations(a => a
        //                                            .Terms("company", c => c.Field(f => f.ExhibitorId).Size(10))
        //                                          )
        //                                          .Query(severalWordsQuery)
        //                                          .Size(maxResults).Skip(page * pageSize).Take(pageSize)
        //                                       );
        //    }
        //    else
        //    {
        //        // b) Gefiltert
        //        //
        //        Dictionary<string, string> fieldMappings = new Dictionary<string, string>()
        //        {
        //            { "company", "exhibitorId"}
        //        };

        //        List<FilterContainer> termFilters = new List<FilterContainer>();
        //        foreach (string key in filters.Keys)
        //        {
        //            termFilters.Add(new TermFilter()
        //            {
        //                Field = fieldMappings[key],
        //                Value = filters[key]
        //            });
        //        }

        //        results = GetClient().Search<BookletUser>(s => s
        //                                     .Aggregations(a => a
        //                                       .Terms("company", c => c.Field(f => f.ExhibitorId).Size(10))

        //                                     )
        //                                     .Query(q => q
        //                                         .Filtered(fq => fq
        //                                            .Query(sq => sq.Bool(b => b.Must(searchQueryList.ToArray())))
        //                                            .Filter(f => f.And(termFilters.ToArray()))
        //                                         )
        //                                     )
        //                                     .Size(maxResults).Skip(page * pageSize).Take(pageSize)
        //                                   );
        //    }
        //    return new SearchResultData() { ResultUsers = results, SearchTerm = SearchTerm };
        //}


    }

}
