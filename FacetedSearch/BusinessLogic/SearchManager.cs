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
            if (SearchTerm == null)
            {
                SearchTerm = ""; // never mind
            }

            Dictionary<string, string> fieldMappings = new Dictionary<string, string>()
            {
                { "language", "languageCode"},
                { "topics", "topics" }
            };


            List<FilterContainer> termFilters = new List<FilterContainer>();
            foreach (string key in filters.Keys)
            {
                if (!fieldMappings.ContainsKey(key))
                {
                    continue; // ignore date range
                }


                termFilters.Add(new TermFilter()
                {
                    Field = fieldMappings[key],
                    Value = filters[key]
                });

            }

            List<QueryContainer> searchQueryList = new List<QueryContainer>();
          
            if (filters.ContainsKey("dateRange"))
            {
                searchQueryList.Add(Query<BlogArticle>.Range(r => r.OnField("timestamp")
                                                                  .Greater(DateTime.Parse(filters["dateRange"])
                                                                  .AddDays(1), "yyyy-MM-dd")));
            }

            // if there are more than one word --> concat using AND 
            //
            string[] splitSearch = SearchTerm.Split(' ');

            
            foreach (var strText in splitSearch)
            {
                searchQueryList.Add(Query<BlogArticle>.Prefix("_all", strText)); // prefix finds incomplete words - use .Term for an exact match
            }


            var results = GetClient().Search<BlogArticle>(s => s

                                             .Aggregations(a => a
                                               .Terms("language", o => o.Field(f => f.LanguageCode).Size(10))
                                               .Terms("topics", o => o.Field(f => f.Topics).Size(10))
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
          

            return new SearchResultViewModel() { Result = results, Filters = filters, SearchTerm = SearchTerm };
        }


    }

}
