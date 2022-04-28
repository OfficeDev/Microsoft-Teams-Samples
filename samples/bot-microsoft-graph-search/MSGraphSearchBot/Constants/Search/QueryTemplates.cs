using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Constants.Search
{
    public static class QueryTemplates
    {
        private const string FilesQuery = "fileType:(aspx OR doc OR docx OR ppt OR pptx OR xls OR xlsx OR pdf OR txt OR one OR vsd OR vsdx)";
        private const string ListItem = "contenttype:Item";
        public static string GetQuery(EntityType entityType,string queryString)
        {
            switch (entityType)
            {
                case EntityType.DriveItem:
                    return $"filename:{queryString} {FilesQuery}";
                case EntityType.ListItem:
                    return $"Title:{queryString} {ListItem}";
                case EntityType.Event:
                case EntityType.Message:
                    return $"Subject:{queryString}";                
                default:
                    return queryString;
                
            }
        }
    }
}
