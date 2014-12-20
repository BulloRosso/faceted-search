using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FacetedSearch.Models;

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
            SearchResultViewModel vm = BusinessLogic.SearchManager.SearchArticle(SearchTerm, 0, GetFilters(), PageSize, MaxResults);

            return View(vm);
        }

        /// <summary>
        /// Second search scenario: simple filter
        /// </summary>
        /// <param name="SearchTerm"></param>
        /// <returns></returns>
        public ActionResult SearchCheckboxed(string SearchTerm)
        {
            return View();
        }

        private Dictionary<string, string> GetFilters()
        {
            if (Session["Filters"] == null)
            {
                Session["Filters"] = new Dictionary<string, string>();
            }

            return (Dictionary<string, string>)Session["Filters"];
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
            Dictionary<string, string> filters = (Dictionary<string, string>)Session["Filters"];

            if (filters.ContainsKey(FilterName))
            {
                filters.Remove(FilterName);
            }

            return RedirectToAction("SearchAdaptive", new { SearchTerm = SearchTerm });
        }

    }
}
