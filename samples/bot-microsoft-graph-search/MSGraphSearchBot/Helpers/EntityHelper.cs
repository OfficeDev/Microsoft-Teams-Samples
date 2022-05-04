using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSGraphSearchSample.Helpers
{
    public static class EntityHelper
    {
        public static string GetValue(Entity entity, string key)
        {
            object val;
            try
            {
                var dictionary = ((ListItem)entity).Fields.AdditionalData;
                var convertedDictionatry = dictionary.ToDictionary(k => k.Key.ToLower(), k => k.Value);

                if (convertedDictionatry.TryGetValue(key.ToLower(), out val))
                {
                    return Convert.ToString(val);
                }
            }
            catch (Exception ex)
            {

            }
            return string.Empty;
        }
        public static string GetListItemValue(Entity entity, string key)
        {
            object val;
            try
            {
                var dictionary = ((ListItem)entity).Fields.AdditionalData;
                var convertedDictionatry = dictionary.ToDictionary(k => k.Key.ToLower(), k => k.Value);

                if (convertedDictionatry.TryGetValue(key.ToLower(), out val))
                {
                    return Convert.ToString(val);
                }
            }
            catch (Exception ex)
            {

            }
            return string.Empty;
        }
    }
}
