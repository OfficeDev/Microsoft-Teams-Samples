using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public TeamsAppPublishingState? Published { get; set; }
        public ITeamsAppAppDefinitionsCollectionPage AppDefinitions { get; set; }
    }
}
