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

<a href="http://www.elasticsearch.org/download/">Download</a> and install elasicsearch. You don't have to configure anything - just start /bin/elasticsearch.bat and the elasticsearch engine starts to listen on its default port 9200. This port is preconfigured in the Visual Studio Project.

<h3>Overview</h3>

Elasticsearch is somewhat similar to mongoDB: both are self contained applications which don't require much administrative efforts (from a developer's perspective). So for example you don't have to set up users or databases before running the Visual Studio Project.

<img src="https://raw.githubusercontent.com/BulloRosso/faceted-search/master/FacetedSearch/Content/img/overview.PNG" style="width:300px" />
