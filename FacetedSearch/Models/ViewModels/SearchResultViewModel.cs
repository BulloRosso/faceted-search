using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FacetedSearch.Models
{
    public class SearchResultViewModel
    {

        public SearchResultViewModel()
        {
            Filters = new Dictionary<string, string>();
        }

        public string SearchTerm { get; set; }
        
        public Dictionary<string, string> Filters { get; set; }

        public ISearchResponse<BlogArticle> Result { get; set; }
        
    }
}