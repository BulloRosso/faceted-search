using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FacetedSearch.Models
{
    public class BlogArticle
    {
        public string Id { get; set; }

        /// <summary>
        /// Blogger's id (ref)
        /// </summary>
        public string BlogAuthorId { get; set; }

        /// <summary>
        /// Two-digit language code (e. g. "en")
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Title of the blog article (e. g. "Setting up SQLServer 2023")
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Short abstract of the article
        /// </summary>
        public string Teaser { get; set; }
    }
}