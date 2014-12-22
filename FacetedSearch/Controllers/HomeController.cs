using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FacetedSearch.Models;
using System.IO;
using Newtonsoft.Json;
using Nest;

namespace FacetedSearch.Controllers
{
    public class HomeController : Controller
    {

        private int MaxResults = 400;
        private int PageSize = 20;

        /// <summary>
        /// Home and explanation
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// First search scenario: drilldown
        /// </summary>
        /// <param name="SearchTerm"></param>
        /// <returns></returns>
        public ActionResult SearchAdaptive(string SearchTerm)
        {

            // Pagination
            //
            int page = 0;
            if (Request.Params["Page"] != null)
            {
                page = Convert.ToInt32(Request.Params["Page"]);
            }

            // elasticsearch 
            //
            SearchResultViewModel vm = BusinessLogic.SearchManager.SearchArticle(SearchTerm, page, GetFilters(), PageSize, MaxResults);

            return View(vm);
        }

        /// <summary>
        /// Reset Index to start new
        /// </summary>
        /// <returns></returns>
        public ActionResult ResetIndex()
        {
            BusinessLogic.SearchManager.ResetIndex();

            string contentFileJSON = Server.MapPath("~/Content/testdata/");

            // read Authors
            //
            using (StreamReader r = new StreamReader(contentFileJSON + "authors.json"))
            {
                string json = r.ReadToEnd();
                List<BlogAuthor> items = JsonConvert.DeserializeObject<List<BlogAuthor>>(json);

                // add to elasticsearch index
                //
                items.ForEach(i => BusinessLogic.SearchManager.Index(i));
            }


            // read articles
            //
            using (StreamReader r = new StreamReader(contentFileJSON + "articles.json"))
            {
                string json = r.ReadToEnd();
                List<BlogArticle> items = JsonConvert.DeserializeObject<List<BlogArticle>>(json);

                // add to elasticsearch index
                //
                items.ForEach(i => BusinessLogic.SearchManager.Index(i));
            }

            return Content("Index reset!");
        }

        /// <summary>
        /// Fake method
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult RedirectToArticle(string Id)
        {
            return Content("In this method you would typically count the clicked search result items and redirect to the final URL.");
        }

        /// <summary>
        /// AutoComplete function (phrase, term or completion in elasticsarch)
        /// </summary>
        /// <param name="SearchTerm"></param>
        /// <returns></returns>
        public ActionResult AutoSuggest(string SearchTerm)
        {
            // use phrase strategy
            //
            ISuggestResponse result = BusinessLogic.SearchManager.AutoSuggest(SearchTerm);

             List<object> lst = new List<object>(); // { new {  name = "test"} , new { name = "fest" } , new { name = "nest" } };

             var sugg = result.Suggestions["completion"].First().Options;
             if (sugg.Count() > 0)
             {
                 foreach (SuggestOption opt in sugg)
                 {
                     lst.Add(new { name = opt.Text });
                 }
             }
           
            return Json(new { search_results = lst },JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Create a new test article
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateArticle(BlogArticle article)
        {
            article.Id = Guid.NewGuid().ToString();
            article.BlogAuthorId = "auth-1";
            
            if (article.Timestamp == null)
            {
                article.Timestamp = DateTime.Now;
            }

            BusinessLogic.SearchManager.Index(article);

            return Json(new { status = "OK" });
        }

        /// <summary>
        /// Second search scenario: simple filter
        /// </summary>
        /// <param name="SearchTerm"></param>
        /// <returns></returns>
        public ActionResult SearchCheckboxed(string SearchTerm)
        {
            // Pagination
            //
            int page = 0;
            if (Request.Params["Page"] != null)
            {
                page = Convert.ToInt32(Request.Params["Page"]);
            }

            // elasticsearch 
            //
            SearchResultViewModel vm = BusinessLogic.SearchManager.SearchArticleCheckboxed(SearchTerm, page, GetCheckboxes(), PageSize, MaxResults);

            return View(vm);
        }

        #region handling of state changes of facets 

        private Dictionary<string, string> GetFilters()
        {
            if (Session["Filters"] == null)
            {
                Session["Filters"] = new Dictionary<string, string>();
            }

            return (Dictionary<string, string>)Session["Filters"];
        }

        private Dictionary<string, string> GetCheckboxes()
        {
            if (Session["Checkboxes"] == null)
            {
                Session["Checkboxes"] = new Dictionary<string, string>();
            }

            return (Dictionary<string, string>)Session["Checkboxes"];
        }

        /// <summary>
        /// Add facet filter and redirect to new search result
        /// </summary>
        /// <param name="SearchTerm"></param>
        /// <param name="FilterName"></param>
        /// <param name="FilterValue"></param>
        /// <returns></returns>
        public ActionResult Filter(string SearchTerm, string FilterName, string FilterValue)
        {
           
            Dictionary<string, string> filters = GetFilters();

            if (!filters.ContainsKey(FilterName))
            {
                filters.Add(FilterName, FilterValue);
            }

            return RedirectToAction("SearchAdaptive", new { SearchTerm = SearchTerm });
        }

        /// <summary>
        /// Remove facet filter and redirect to new search result
        /// </summary>
        /// <param name="SearchTerm"></param>
        /// <param name="FilterName"></param>
        /// <param name="FilterValue"></param>
        /// <returns></returns>
        public ActionResult RemoveFilter(string SearchTerm, string FilterName, string FilterValue)
        {
            Dictionary<string, string> filters = GetFilters();

            if (filters.ContainsKey(FilterName))
            {
                filters.Remove(FilterName);
            }

            return RedirectToAction("SearchAdaptive", new { SearchTerm = SearchTerm });
        }

        /// <summary>
        /// Add facet checkbox and redirect to new search result
        /// </summary>
        /// <param name="SearchTerm"></param>
        /// <param name="FilterName"></param>
        /// <param name="FilterValue"></param>
        /// <returns></returns>
        public ActionResult Checkbox(string SearchTerm, string FilterName, string FilterValue)
        {

            Dictionary<string, string> filters = GetCheckboxes();

            if (!filters.ContainsKey(FilterName))
            {
                filters.Add(FilterName, "," + FilterValue);
            }
            else
            {
                filters[FilterName] = filters[FilterName] + "," + FilterValue;
            }

            return RedirectToAction("SearchCheckboxed", new { SearchTerm = SearchTerm });
        }

        /// <summary>
        /// Remove facet checkbox and redirect to new search result
        /// </summary>
        /// <param name="SearchTerm"></param>
        /// <param name="FilterName"></param>
        /// <param name="FilterValue"></param>
        /// <returns></returns>
        public ActionResult RemoveCheckbox(string SearchTerm, string FilterName, string FilterValue)
        {
            Dictionary<string, string> filters = GetCheckboxes();

            if (filters.ContainsKey(FilterName))
            {
                if (filters[FilterName] == "," + FilterValue)
                {
                    filters.Remove(FilterName);
                }
                else
                {
                    filters[FilterName] = filters[FilterName].Replace("," + FilterValue, "");
                }
            }

            return RedirectToAction("SearchCheckboxed", new { SearchTerm = SearchTerm });
        }

        #endregion
    }
}
