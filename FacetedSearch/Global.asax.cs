using FacetedSearch.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace FacetedSearch
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Create elasticsearch index
            SetUpDemoData();
        }

        /// <summary>
        /// Read demo data from JSON-Files in /content/ and create elasticsearch index from it
        /// </summary>
        private void SetUpDemoData()
        {

            string contentFileJSON = Server.MapPath("~/Content/testdata/");

            // read Authors
            //
            using (StreamReader r = new StreamReader(contentFileJSON + "authors.json"))
            {
                string json = r.ReadToEnd();
                List<BlogAuthor> items = JsonConvert.DeserializeObject<List<BlogAuthor>>(json);

            }


            // read articles
            //
            using (StreamReader r = new StreamReader(contentFileJSON + "articles.json"))
            {
                string json = r.ReadToEnd();
                List<BlogArticle> items = JsonConvert.DeserializeObject<List<BlogArticle>>(json);

            }

        }
    }
}