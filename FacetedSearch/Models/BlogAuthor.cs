using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FacetedSearch.Models
{
    public class BlogAuthor
    {
        public string Id { get; set; }

        /// <summary>
        /// Blogger's real name or stage name
        /// </summary>
        public string DisplayName { get; set; }

        public string EMail { get; set; }

        /// <summary>
        /// Two-digit country code (e. g. "DE")
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Which topics does the author blog about (e. g. "Fashion", "C#")?
        /// </summary>
        public List<string> TopicCodes { get; set; }
    }
}