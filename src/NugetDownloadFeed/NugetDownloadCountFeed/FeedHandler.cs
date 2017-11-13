using NugetDownloadCountFeed.ServiceReference1;
using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;

namespace NugetDownloadCountFeed
{
    public class FeedHandler : IHttpHandler
    {
        private const string GalleryUri = "https://marketplace.visualstudio.com/";
        //private readonly IDictionary<string, IList<SyndicationItem>> packageDownloadCounts = new ConcurrentDictionary<string, IList<SyndicationItem>>();

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //Hardcoded for SQLite Toolbox VsixId!

            var items = Search(new VsIdeServiceClient());

            if (items.Length > 0)
            {
                var foundItem = items[0];

	            string version;
	            if (!foundItem.Project.Metadata.TryGetValue("VsixVersion", out version))
	            {
		            version = "0.0.0.0";
	            }

                List<SyndicationItem> feedItems = new List<SyndicationItem>();
                var title = foundItem.Project.Title;
                feedItems.Add(new SyndicationItem(
                            title,
                            (foundItem.Files[0].DownloadCount).ToString(),
                            new Uri(GalleryUri),
                            Guid.NewGuid().ToString(),
                            new DateTimeOffset(DateTime.UtcNow)));                    

                var feed = new SyndicationFeed("VS Marketplace Download Count Feed",
                                                version,
                                                new Uri(GalleryUri), GalleryUri, foundItem.Project.ModifiedDate,
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

        private Release[] Search(VsIdeServiceClient client)
        {
            var whereClause = "(Project.MetaData['VsixId'] = '41521019-e4c7-480c-8ea8-fc4a2c6f50aa') AND (Project.Metadata['SupportedVSEditions'] LIKE '%15.0.26430.16,Pro;%')";
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
