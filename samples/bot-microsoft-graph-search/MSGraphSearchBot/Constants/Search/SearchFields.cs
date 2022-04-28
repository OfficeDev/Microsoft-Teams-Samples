using Microsoft.Graph;
using System.Collections.Generic;

namespace MSGraphSearchSample.Constants.Search
{
    public static class SearchFields
    {
        public static List<string> ListItemFields = new List<string>()
        {
            "title",
            "listItem",
            "author",
            "created",
            "createdby",
            "lastmodifiedtime",
            "url",
            "sitetitle",
            "sitepath"
        };
        public static List<string> MessageFields = new List<string>()
        {
            "subject",
            "importance",
            "hasattachments",
            "createddatetime",
            "from",
            "isRead",
            "webLink"
        };
        public static List<string> EventFields = new List<string>()
        {
            "summary",
            "start",
            "end",
            "subject",
            "isAllDay",
            "sensitivity"
        };
        public static List<string> DriveItemFields = new List<string>()
        {
            "createdDateTime",
            "name",
            "webUrl",
            "createdBy",
            "size"
        };
        public static List<string> GetFieldsByEntityType(EntityType entity)
        {
            switch (entity)
            {
                case EntityType.ListItem:
                    return ListItemFields;
                case EntityType.Event:
                    return EventFields;
                case EntityType.DriveItem:
                    return DriveItemFields;
                case EntityType.Message:
                    return MessageFields;
                default:
                    return null;
            }
        }
    }
}
