using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;

namespace AppCatalogSample.Helper
{
    public class CardData
    {
        public string DisplayName { get; set; }
        public string DistributionMethod { get; set; }
        public string ExternalId { get; set; }
        public string Id { get; set; }
        public string OdataType { get; set; }
        public string AdditionalData { get; set; }

        // Publishing state of the app
        public TeamsAppPublishingState? Published { get; set; }

        // Updated for SDK v5: use List<TeamsAppDefinition> instead of ITeamsAppAppDefinitionsCollectionPage
        public List<TeamsAppDefinition> AppDefinitions { get; set; }
    }
}
