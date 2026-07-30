// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Solutions.Responses
{
    // Compatibility wrapper for legacy samples expecting LocaleTemplateEngineManager.
    public class LocaleTemplateEngineManager
    {
        private readonly MultiLanguageLG _multiLanguageLg;
        private readonly string _defaultLocale;

        public LocaleTemplateEngineManager(Dictionary<string, List<string>> localizedTemplates, string defaultLocale)
        {
            _defaultLocale = string.IsNullOrWhiteSpace(defaultLocale) ? "en-us" : defaultLocale.ToLowerInvariant();

            var filesPerLocale = localizedTemplates
                .Where(kvp => kvp.Value != null && kvp.Value.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key.ToLowerInvariant(),
                    kvp => kvp.Value[0]);

            _multiLanguageLg = new MultiLanguageLG(filesPerLocale, _defaultLocale);
        }

        public Activity GenerateActivityForLocale(string templateName, object data = null)
        {
            var locale = CultureInfo.CurrentUICulture?.Name?.ToLowerInvariant() ?? _defaultLocale;
            var generated = _multiLanguageLg.Generate(templateName, data, locale);

            if (generated is Activity activity)
            {
                return activity;
            }

            if (generated is JObject json)
            {
                return json.ToObject<Activity>() ?? MessageFactory.Text(json.ToString()) as Activity;
            }

            return MessageFactory.Text(generated?.ToString() ?? string.Empty) as Activity;
        }
    }
}
