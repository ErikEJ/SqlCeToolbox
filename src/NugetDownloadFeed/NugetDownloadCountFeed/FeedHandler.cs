using NugetDownloadCountFeed.ServiceReference1;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace NugetDownloadCountFeed
{
    public class FeedHandler : IHttpHandler
    {
        private const string GalleryUri = "http://visualstudiogallery.msdn.microsoft.com/";
        private readonly IDictionary<string, IList<SyndicationItem>> packageDownloadCounts = new ConcurrentDictionary<string, IList<SyndicationItem>>();

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var packageId = context.Request.QueryString["extensionId"];
            Guid id;

            if (string.IsNullOrWhiteSpace(packageId) || !Guid.TryParse(packageId, out id))
            {
                context.Response.StatusCode = 404;
                context.Response.StatusDescription = "Missing or invalid extensionId";                
                context.Response.End();
                return;
            }
            var items = Search(new VsIdeServiceClient(), id);

            if (items.Length > 0)
            {
                var foundItem = items[0];
                var nugetUrl = string.Format(
                    "{0}{1}", GalleryUri, id);

	            string version;
	            if (!foundItem.Project.Metadata.TryGetValue("VsixVersion", out version))
	            {
		            version = "0.0.0.0";
	            }

                List<SyndicationItem> feedItems = new List<SyndicationItem>();
                var title = foundItem.Project.Title;
                feedItems.Add(new SyndicationItem(
                            title,
                            (foundItem.Files[0].DownloadCount - 78000).ToString(),
                            new Uri(nugetUrl),
                            Guid.NewGuid().ToString(),
                            new DateTimeOffset(DateTime.UtcNow)));                    

                var feed = new SyndicationFeed("VS Gallery Download Count Feed",
                                                version,
                                                new Uri(nugetUrl), nugetUrl, foundItem.Project.ModifiedDate,
                                                feedItems);
                using (var xmlWriter = XmlWriter.Create(context.Response.OutputStream))
                {
                    feed.SaveAsRss20(xmlWriter);
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
                context.Response.ContentType = "text/xml";
                context.Response.End();
            }
        }

        private Release[] Search(VsIdeServiceClient client, Guid id)
        {
            var whereClause = string.Format("(Project.Metadata['SupportedVSEditions'] LIKE '%15.0.26430.16,Pro;%')", id);
            var orderClause = "Project.Metadata['Ranking'] desc";
            var requestContext = new Dictionary<string, string>()
            {
                { "LCID", "1033" },
                {"SearchSource", "ExtensionManagerQuery"},
                {"OSVersion","10.0.15063.0"}
            };

            var result = client.SearchReleases2("ErikEJ.SQLServerCompactSQLiteToolbox", whereClause, orderClause, 0, 1, requestContext);
            return result.Releases;
        }
    }
}
