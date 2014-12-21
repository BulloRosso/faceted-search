<h2>Faceted Search with Elasticsearch and C#/NEST</h2>

<h3>What is this all about?</h3>

<p>This is a MVC 4 Visual Studio 2013 project using Twitter Bootstrap to showcase a basic faceted search UI.</p>

<p>It'll get you started with a typical search and solve some of the most common problems like paging, filtering and hit highlighting:</p>

<img src="https://raw.githubusercontent.com/BulloRosso/faceted-search/master/FacetedSearch/Content/img/sampleSearch.PNG" style="width:300px" />

To simplyfy things you don't need any kind of database - sample data is included as plain JSON text file.

<h3>Components / Dependencies</h3>
<ul>
<li>Elasticsearch search engine</li>
<li>Twitter Bootstrap 3</li>
<li>Nest Wrapper for Elasticsearch</li>

</ul>

<h3>Installing Elasticsearch</h3>

<a href="http://www.elasticsearch.org/download/">Download</a> and install elasticsearch. You don't have to configure anything - just start <code>/bin/elasticsearch.bat</code> and the elasticsearch engine starts to listen on its default port 9200. This port is preconfigured in the Visual Studio Project.

<h3>Overview</h3>

Elasticsearch is somewhat similar to mongoDB: both are self contained applications which don't require much administrative efforts (from a developer's perspective). So for example you don't have to set up users or databases before running the Visual Studio Project.

<img src="https://raw.githubusercontent.com/BulloRosso/faceted-search/master/FacetedSearch/Content/img/overview.PNG" style="width:300px" />

While we'd be able to send/receive plain JSON data structures to communicate with the elasticsearch engine we'll make use of the Nest project: Nest is a fluent wrapper for elasticsearch and allows us to use LINQ or C#-Types without too much hassle with technical details.

<h3>Demo Domain Data</h3>

You can search blog entries of different languages which are assigned to several topics (like "Fashion &amp; Lifestyle", "Music &amp; Entertainment").

<img src="https://raw.githubusercontent.com/BulloRosso/faceted-search/master/FacetedSearch/Content/img/BlogArticleClass.PNG"  />

<h3>Drilldown strategies</h3>

After entering one or more search terms in the search box elasticsearch returns a rich set of results. By using aggregations (elasticsearchs facets on steroids) different options to narrow the results are available:

<ul>
   <li>Language of the blog article</li>
   <li>Topic of the blog article</li>
   <li>Date the blog article was created</li>
</ul>

Depending on the problem domain you can choose between one of the following drilldown strategies:

<ol>
    <li><strong>Adaptive facets</strong><br/>
    Each choice will lead to a new resultset and all facets will be calculated upon the new results. By using this
    approach it is ensured there is always at least one result in the result set. 
    </li>
   <li><strong>Checkbox facets</strong><br/>
Each choice will lead to a new resultset but the initial facets will remain unchanged. This strategy will enable
the user to create OR queries like "all articles in english OR french language" - but it's easy to end up with the
dreaded "no blog articles matched your search criteria" screen.
</li>
</ol>

<h3>Don't let demand managers spoil your search experience</h3>

In order to illustrate how easy demand management will be able to "destroy" your search experience I've included a "wrong" facet in the adaptive facets example: the date facet is just a simple date picker which allows the user to pick any date. This can easily lead to the "no blog articles matched your search criteria" screen.

The "right" facet would be to include pre-compiled date ranges provided by elasticsearch's aggregations feature - this one is included in the checkbox facets example.

<h3>Other common search features</h3>

Still to do...

<ul>
   <li>"Did you mean..?" for typos</li>
   <li>Auto complete feature in the search input box</li>
</ul>
